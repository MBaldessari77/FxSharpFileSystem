using System;
using System.IO;
using System.Linq;
using System.Text;
using SharpFileSystem.FileSystems;
using SharpFileSystem.IO;
using Xunit;

namespace SharpFileSystem.Tests.FileSystems
{
	public class PhysicalFileSystemTest : IDisposable
	{
		public PhysicalFileSystemTest()
		{
			FileName = "x";
			FileNamePath = FileSystemPath.Root.AppendFile(FileName);
			Root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
			System.IO.Directory.CreateDirectory(Root);
			AbsoluteFileName = Path.Combine(Root, FileName);
			FileSystem = new PhysicalFileSystem(Root);
		}

		public void Dispose()
		{
			FileSystem?.Dispose();
			System.IO.Directory.Delete(Root, true);
		}

		string Root { get; }
		PhysicalFileSystem FileSystem { get; }
		string AbsoluteFileName { get; }

		string FileName { get; }
		FileSystemPath FileNamePath { get; }

		[Fact]
		public void CreateFile()
		{
			Assert.False(System.IO.File.Exists(AbsoluteFileName));
			Assert.False(FileSystem.Exists(FileNamePath));

			var content = Encoding.UTF8.GetBytes("asdf");
			using (var stream = FileSystem.CreateFile(FileNamePath))
			{
				// File should exist at this point.
				Assert.True(FileSystem.Exists(FileNamePath));
				// File should also exist irl at this point.
				Assert.True(System.IO.File.Exists(AbsoluteFileName));

				stream.Write(content, 0, content.Length);
			}

			// File should contain content.
			Assert.Equal(content, System.IO.File.ReadAllBytes(AbsoluteFileName));

			using (var stream = FileSystem.OpenFile(FileNamePath, FileAccess.Read))
			{
				// Verify that EOF type stuff works.
				var readContent = new byte[2 * content.Length];
				Assert.Equal(content.Length, stream.Read(readContent, 0, readContent.Length));
				Assert.Equal(
					content,
					// trim to actual length.
					readContent.Take(content.Length).ToArray());

				// Trying to read beyond end of file should just return 0.
				Assert.Equal(0, stream.Read(readContent, 0, readContent.Length));
			}
		}

		[Fact]
		public void CreateFile_Empty()
		{
			// ReSharper disable once UnusedVariable
			using (var stream = FileSystem.CreateFile(FileNamePath))
			{
			}

			Assert.Equal(
				new byte[] { },
				System.IO.File.ReadAllBytes(AbsoluteFileName));
			using (var stream = FileSystem.OpenFile(FileNamePath, FileAccess.Read))
			{
				Assert.Equal(
					new byte[] { },
					stream.ReadAllBytes());
			}
		}

		[Fact]
		public void CreateFile_Exists()
		{
			Assert.False(System.IO.File.Exists(AbsoluteFileName));
			Assert.False(FileSystem.Exists(FileNamePath));

			using (var stream = FileSystem.CreateFile(FileNamePath))
			{
				var content1 = Encoding.UTF8.GetBytes("asdf");
				stream.Write(content1, 0, content1.Length);
			}

			// creating an existing file should truncate like open(O_CREAT).
			var content2 = Encoding.UTF8.GetBytes("b");
			using (var stream = FileSystem.CreateFile(FileNamePath))
			{
				stream.Write(content2, 0, content2.Length);
			}

			Assert.Equal(content2, System.IO.File.ReadAllBytes(AbsoluteFileName));
			using (var stream = FileSystem.OpenFile(FileNamePath, FileAccess.Read))
			{
				Assert.Equal(content2, stream.ReadAllBytes());
			}
		}
	}
}
