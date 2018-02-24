using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SharpFileSystem.Tests
{
	/// <summary>
	///     This is a test class for FileSystemPathTest and is intended
	///     to contain all FileSystemPathTest Unit Tests
	/// </summary>
	public class FileSystemPathTest
	{
		readonly FileSystemPath[] _paths =
		{
			_root,
			DirectoryA,
			_fileA,
			_directoryB,
			_fileB
		};

		IEnumerable<FileSystemPath> Directories { get { return _paths.Where(p => p.IsDirectory); } }
		IEnumerable<FileSystemPath> Files { get { return _paths.Where(p => p.IsFile); } }

		static readonly FileSystemPath DirectoryA = FileSystemPath.Parse("/directorya/");
		static FileSystemPath _fileA = FileSystemPath.Parse("/filea");
		static FileSystemPath _directoryB = FileSystemPath.Parse("/directorya/directoryb/");
		static FileSystemPath _fileB = FileSystemPath.Parse("/directorya/fileb.txt");
		static FileSystemPath _root = FileSystemPath.Root;
		FileSystemPath _fileC;

		/// <summary>
		///     A test for AppendDirectory
		/// </summary>
		[Fact]
		public void AppendDirectoryTest()
		{
			foreach (var d in Directories)
				Assert.True(d.AppendDirectory("dir").IsDirectory);
			foreach (var d in Directories)
				Assert.True(d.AppendDirectory("dir").EntityName == "dir");
			foreach (var d in Directories)
				Assert.True(d.AppendDirectory("dir").ParentPath == d);
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			EAssert.Throws<InvalidOperationException>(() => _fileA.AppendDirectory("dir"));
			EAssert.Throws<ArgumentException>(() => _root.AppendDirectory("dir/dir"));
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed
		}

		/// <summary>
		///     A test for AppendFile
		/// </summary>
		[Fact]
		public void AppendFileTest()
		{
			foreach (var d in Directories)
				Assert.True(d.AppendFile("file").IsFile);
			foreach (var d in Directories)
				Assert.True(d.AppendFile("file").EntityName == "file");
			foreach (var d in Directories)
				Assert.True(d.AppendFile("file").ParentPath == d);
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			EAssert.Throws<InvalidOperationException>(() => _fileA.AppendFile("file"));
			EAssert.Throws<ArgumentException>(() => DirectoryA.AppendFile("dir/file"));
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed
		}

		/// <summary>
		///     A test for AppendPath
		/// </summary>
		[Fact]
		public void AppendPathTest()
		{
			Assert.True(Directories.All(p => p.AppendPath(_root) == p));
			Assert.True(Directories.All(p => p.AppendPath("") == p));

			var subpath = FileSystemPath.Parse("/dir/file");
			var subpathstr = "dir/file";
			foreach (var p in Directories)
				Assert.True(p.AppendPath(subpath).ParentPath.ParentPath == p);
			foreach (var p in Directories)
				Assert.True(p.AppendPath(subpathstr).ParentPath.ParentPath == p);
			foreach (var pa in Directories)
			foreach (var pb in _paths.Where(pb => !pb.IsRoot))
				Assert.True(pa.AppendPath(pb).IsChildOf(pa));
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			EAssert.Throws<InvalidOperationException>(() => _fileA.AppendPath(subpath));
			EAssert.Throws<InvalidOperationException>(() => _fileA.AppendPath(subpathstr));
			EAssert.Throws<ArgumentException>(() => DirectoryA.AppendPath("/rootedpath/"));
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed
		}

		/// <summary>
		///     A test for ChangeExtension
		/// </summary>
		[Fact]
		public void ChangeExtensionTest()
		{
			foreach (var p in _paths.Where(p => p.IsFile))
				Assert.True(p.ChangeExtension(".exe").GetExtension() == ".exe");
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			EAssert.Throws<ArgumentException>(() => DirectoryA.ChangeExtension(".exe"));
		}


		/// <summary>
		///     A test for CompareTo
		/// </summary>
		[Fact]
		public void CompareToTest()
		{
			foreach (var pa in _paths)
			foreach (var pb in _paths)
				Assert.Equal(Math.Sign(pa.CompareTo(pb)), Math.Sign(string.Compare(pa.ToString(), pb.ToString(), StringComparison.Ordinal)));
		}

		/// <summary>
		///     A test for EntityName
		/// </summary>
		[Fact]
		public void EntityNameTest()
		{
			Assert.Equal("filea", _fileA.EntityName);
			Assert.Equal("fileb.txt", _fileB.EntityName);
			Assert.Null(_root.EntityName);
		}

		/// <summary>
		///     A test for GetDirectorySegments
		/// </summary>
		[Fact]
		public void GetDirectorySegmentsTest()
		{
			Assert.Empty(_root.GetDirectorySegments());
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			Directories
				.Where(d => !d.IsRoot)
				.All(d => d.GetDirectorySegments().Length == d.ParentPath.GetDirectorySegments().Length - 1);
			Files.All(f => f.GetDirectorySegments().Length == f.ParentPath.GetDirectorySegments().Length);
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed
		}

		/// <summary>
		///     A test for GetExtension
		/// </summary>
		[Fact]
		public void GetExtensionTest()
		{
			Assert.Equal("", _fileA.GetExtension());
			Assert.Equal(".txt", _fileB.GetExtension());
			_fileC = FileSystemPath.Parse("/directory.txt/filec");
			Assert.Equal("", _fileC.GetExtension());
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			EAssert.Throws<ArgumentException>(() => DirectoryA.GetExtension());
		}

		/// <summary>
		///     A test for IsChildOf
		/// </summary>
		[Fact]
		public void IsChildOfTest()
		{
			Assert.True(_fileB.IsChildOf(DirectoryA));
			Assert.True(_directoryB.IsChildOf(DirectoryA));
			Assert.True(_fileA.IsChildOf(_root));
			Assert.True(DirectoryA.IsChildOf(_root));
			Assert.True(_fileB.IsChildOf(_root));
			Assert.True(_directoryB.IsChildOf(_root));

			Assert.False(DirectoryA.IsChildOf(_fileB));
			Assert.False(DirectoryA.IsChildOf(_directoryB));
			Assert.False(_root.IsChildOf(_fileA));
			Assert.False(_root.IsChildOf(DirectoryA));
			Assert.False(_root.IsChildOf(_fileB));
			Assert.False(_root.IsChildOf(_directoryB));
		}

		/// <summary>
		///     A test for IsDirectory
		/// </summary>
		[Fact]
		public void IsDirectoryTest()
		{
			Assert.True(DirectoryA.IsDirectory);
			Assert.True(_root.IsDirectory);
			Assert.False(_fileA.IsDirectory);
		}

		/// <summary>
		///     A test for IsFile
		/// </summary>
		[Fact]
		public void IsFileTest()
		{
			Assert.True(_fileA.IsFile);
			Assert.False(DirectoryA.IsFile);
			Assert.False(_root.IsFile);
		}

		/// <summary>
		///     A test for IsParentOf
		/// </summary>
		[Fact]
		public void IsParentOfTest()
		{
			Assert.True(DirectoryA.IsParentOf(_fileB));
			Assert.True(DirectoryA.IsParentOf(_directoryB));
			Assert.True(_root.IsParentOf(_fileA));
			Assert.True(_root.IsParentOf(DirectoryA));
			Assert.True(_root.IsParentOf(_fileB));
			Assert.True(_root.IsParentOf(_directoryB));

			Assert.False(_fileB.IsParentOf(DirectoryA));
			Assert.False(_directoryB.IsParentOf(DirectoryA));
			Assert.False(_fileA.IsParentOf(_root));
			Assert.False(DirectoryA.IsParentOf(_root));
			Assert.False(_fileB.IsParentOf(_root));
			Assert.False(_directoryB.IsParentOf(_root));
		}

		/// <summary>
		///     A test for IsRooted
		/// </summary>
		[Fact]
		public void IsRootedTest()
		{
			Assert.True(FileSystemPath.IsRooted("/filea"));
			Assert.True(FileSystemPath.IsRooted("/directorya/"));
			Assert.False(FileSystemPath.IsRooted("filea"));
			Assert.False(FileSystemPath.IsRooted("directorya/"));
			Assert.True(_paths.All(p => FileSystemPath.IsRooted(p.ToString())));
		}

		/// <summary>
		///     A test for IsRoot
		/// </summary>
		[Fact]
		public void IsRootTest()
		{
			Assert.True(_root.IsRoot);
			Assert.False(DirectoryA.IsRoot);
			Assert.False(_fileA.IsRoot);
		}

		/// <summary>
		///     A test for ParentPath
		/// </summary>
		[Fact]
		public void ParentPathTest()
		{
			Assert.True(
				Directories
					.Where(d => d.GetDirectorySegments().Length == 1)
					.All(d => d.ParentPath == _root)
			);

			Assert.False(Files != null && Files.Any(f => f.RemoveChild(_root.AppendFile(f.EntityName)) != f.ParentPath));
			EAssert.Throws<InvalidOperationException>(() => Assert.Equal(_root.ParentPath, _root.ParentPath));
		}

		/// <summary>
		///     A test for Parse
		/// </summary>
		[Fact]
		public void ParseTest()
		{
			Assert.True(_paths.All(p => p == FileSystemPath.Parse(p.ToString())));
			EAssert.Throws<ArgumentNullException>(() => FileSystemPath.Parse(null));
			EAssert.Throws<ParseException>(() => FileSystemPath.Parse("thisisnotapath"));
			EAssert.Throws<ParseException>(() => FileSystemPath.Parse("/thisisainvalid//path"));
		}

		/// <summary>
		///     A test for RemoveChild
		/// </summary>
		[Fact]
		public void RemoveChildTest()
		{
			Assert.Equal(DirectoryA, _fileB.RemoveChild(FileSystemPath.Parse("/fileb.txt")));
			Assert.Equal(DirectoryA, _directoryB.RemoveChild(FileSystemPath.Parse("/directoryb/")));
			Assert.Equal(_root, _directoryB.RemoveChild(_directoryB));
			Assert.Equal(_root, _fileB.RemoveChild(_fileB));
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			EAssert.Throws<ArgumentException>(() => DirectoryA.RemoveChild(FileSystemPath.Parse("/nonexistantchild")));
			EAssert.Throws<ArgumentException>(() => DirectoryA.RemoveChild(FileSystemPath.Parse("/directorya")));
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed
		}

		/// <summary>
		///     A test for RemoveParent
		/// </summary>
		[Fact]
		public void RemoveParentTest()
		{
			Assert.Equal(_root, _directoryB.RemoveParent(_directoryB));
			Assert.Equal(FileSystemPath.Parse("/fileb.txt"), _fileB.RemoveParent(DirectoryA));
			Assert.Equal(_root, _root.RemoveParent(_root));
			Assert.Equal(_directoryB, _directoryB.RemoveParent(_root));
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			EAssert.Throws<ArgumentException>(() => _fileB.RemoveParent(FileSystemPath.Parse("/nonexistantparent/")));
			EAssert.Throws<ArgumentException>(() => _fileB.RemoveParent(FileSystemPath.Parse("/nonexistantparent")));
			EAssert.Throws<ArgumentException>(() => _fileB.RemoveParent(FileSystemPath.Parse("/fileb.txt")));
			EAssert.Throws<ArgumentException>(() => _fileB.RemoveParent(FileSystemPath.Parse("/directorya")));
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed
		}

		/// <summary>
		///     A test for Root
		/// </summary>
		[Fact]
		public void RootTest() { Assert.Equal(_root, FileSystemPath.Parse("/")); }

		/// <summary>
		///     A test for ToString
		/// </summary>
		[Fact]
		public void ToStringTest()
		{
			var s = "/directorya/";
			Assert.Equal(s, FileSystemPath.Parse(s).ToString());
		}
	}
}
