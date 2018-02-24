using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpFileSystem.FileSystems
{
	// ReSharper disable once UnusedMember.Global
	public abstract class SeamlessArchiveFileSystem : IFileSystem
	{
		const char ArchiveDirectorySeparator = '#';

		readonly FileSystemUsage _rootUsage;
		readonly IDictionary<File, FileSystemUsage> _usedArchives = new Dictionary<File, FileSystemUsage>();

		protected SeamlessArchiveFileSystem(IFileSystem fileSystem)
		{
			FileSystem = fileSystem;
			_rootUsage = new FileSystemUsage
			{
				Owner = this,
				FileSystem = FileSystem,
				ArchiveFile = null
			};
		}

		IFileSystem FileSystem { get; }

		public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
		{
			using (var r = Refer(path))
			{
				var fileSystem = r.FileSystem;

				FileSystemPath parentPath;
				parentPath = TryGetArchivePath(path, out parentPath) ? ArchiveFileToDirectory(parentPath) : FileSystemPath.Root;
				var entities = new LinkedList<FileSystemPath>();
				foreach (var ep in fileSystem.GetEntities(GetRelativePath(path)))
				{
					var newep = parentPath.AppendPath(ep.ToString().Substring(1));
					entities.AddLast(newep);
					if (IsArchiveFile(fileSystem, newep))
						entities.AddLast(newep.ParentPath.AppendDirectory(newep.EntityName + ArchiveDirectorySeparator));
				}

				return entities;
			}
		}

		public bool Exists(FileSystemPath path)
		{
			using (var r = Refer(path))
			{
				var fileSystem = r.FileSystem;
				return fileSystem.Exists(GetRelativePath(path));
			}
		}

		public Stream OpenFile(FileSystemPath path, FileAccess access)
		{
			var r = Refer(path);
			var s = r.FileSystem.OpenFile(GetRelativePath(path), access);
			return new SafeReferenceStream(s, r);
		}

		public void Dispose()
		{
			foreach (var reference in _usedArchives.Values.SelectMany(usage => usage.References).ToArray())
				UnuseFileSystem(reference);
			FileSystem.Dispose();
		}

		void UnuseFileSystem(FileSystemReference reference)
		{
			// When root filesystem was used.
			if (reference.Usage.ArchiveFile == null)
				return;

			if (!_usedArchives.TryGetValue(reference.Usage.ArchiveFile, out var usage))
				throw new ArgumentException("The specified reference is not valid.");
			if (!usage.References.Remove(reference))
				throw new ArgumentException("The specified reference does not exist.");
			if (usage.References.Count == 0)
			{
				_usedArchives.Remove(usage.ArchiveFile);

				usage.FileSystem.Dispose();
			}
		}

		protected abstract bool IsArchiveFile(IFileSystem fileSystem, FileSystemPath path);

		FileSystemPath ArchiveFileToDirectory(FileSystemPath path)
		{
			if (!path.IsFile)
				throw new ArgumentException("The specified path is not a file.");
			return path.ParentPath.AppendDirectory(path.EntityName + ArchiveDirectorySeparator);
		}

		FileSystemPath GetRelativePath(FileSystemPath path)
		{
			var s = path.ToString();
			var sindex = s.LastIndexOf($"{ArchiveDirectorySeparator}{ArchiveDirectorySeparator}", StringComparison.OrdinalIgnoreCase);
			if (sindex < 0)
				return path;
			return FileSystemPath.Parse(s.Substring(sindex + 1));
		}

		// ReSharper disable once UnusedMember.Global
		protected bool HasArchive(FileSystemPath path) { return path.ToString().LastIndexOf($"{ArchiveDirectorySeparator}{ArchiveDirectorySeparator}", StringComparison.OrdinalIgnoreCase) >= 0; }

		bool TryGetArchivePath(FileSystemPath path, out FileSystemPath archivePath)
		{
			var p = path.ToString();
			var sindex = p.LastIndexOf($"{ArchiveDirectorySeparator}{ArchiveDirectorySeparator}", StringComparison.OrdinalIgnoreCase);
			if (sindex < 0)
			{
				archivePath = path;
				return false;
			}

			archivePath = FileSystemPath.Parse(p.Substring(0, sindex));
			return true;
		}

		FileSystemReference Refer(FileSystemPath path)
		{
			if (TryGetArchivePath(path, out var archivePath))
				return CreateArchiveReference(archivePath);
			return new FileSystemReference(_rootUsage);
		}

		FileSystemReference CreateArchiveReference(FileSystemPath archiveFile) { return CreateReference((File) GetActualLocation(archiveFile)); }

		FileSystemReference CreateReference(File file)
		{
			var usage = GetArchiveFs(file);
			var reference = new FileSystemReference(usage);
			usage.References.Add(reference);
			return reference;
		}

		FileSystemEntity GetActualLocation(FileSystemPath path)
		{
			if (!TryGetArchivePath(path, out var archivePath))
				return FileSystemEntity.Create(FileSystem, path);
			var archiveFile = (File) GetActualLocation(archivePath);
			var usage = GetArchiveFs(archiveFile);
			return FileSystemEntity.Create(usage.FileSystem, GetRelativePath(path));
		}

		FileSystemUsage GetArchiveFs(File archiveFile)
		{
			if (_usedArchives.TryGetValue(archiveFile, out var usage)) return usage;

			var archiveFs = CreateArchiveFileSystem(archiveFile);
			usage = new FileSystemUsage
			{
				Owner = this,
				FileSystem = archiveFs,
				ArchiveFile = archiveFile
			};
			_usedArchives[archiveFile] = usage;
			//System.Diagnostics.Debug.WriteLine("Open archives: " + _usedArchives.Count);
			return usage;
		}

		protected abstract IFileSystem CreateArchiveFileSystem(File archiveFile);

		// ReSharper disable once UnusedMember.Global
		public class DummyDisposable : IDisposable
		{
			public void Dispose() { }
		}

		public class FileSystemReference : IDisposable
		{
			public FileSystemReference(FileSystemUsage usage) { Usage = usage; }

			public FileSystemUsage Usage { get; }
			public IFileSystem FileSystem => Usage.FileSystem;
			public void Dispose() { Usage.Owner.UnuseFileSystem(this); }
		}

		public class FileSystemUsage
		{
			public FileSystemUsage() { References = new LinkedList<FileSystemReference>(); }

			public SeamlessArchiveFileSystem Owner { get; set; }
			public File ArchiveFile { get; set; }
			public IFileSystem FileSystem { get; set; }
			public ICollection<FileSystemReference> References { get; }
		}

		class SafeReferenceStream : Stream
		{
			readonly FileSystemReference _reference;
			readonly Stream _stream;

			public SafeReferenceStream(Stream stream, FileSystemReference reference)
			{
				_stream = stream;
				_reference = reference;
			}

			public override bool CanRead => _stream.CanRead;

			public override bool CanSeek => _stream.CanSeek;

			public override bool CanWrite => _stream.CanWrite;

			public override long Length => _stream.Length;

			public override long Position { get => _stream.Position; set => _stream.Position = value; }

			public override void Flush() { _stream.Flush(); }

			public override long Seek(long offset, SeekOrigin origin) { return _stream.Seek(offset, origin); }

			public override void SetLength(long value) { _stream.SetLength(value); }

			public override int Read(byte[] buffer, int offset, int count) { return _stream.Read(buffer, offset, count); }

			public override void Write(byte[] buffer, int offset, int count) { _stream.Write(buffer, offset, count); }

			public override void Close()
			{
				_stream.Close();
				_reference.Dispose();
			}
		}

		#region Not implemented

		public Stream CreateFile(FileSystemPath path)
		{
			var r = Refer(path);
			var s = r.FileSystem.CreateFile(GetRelativePath(path));
			return new SafeReferenceStream(s, r);
		}

		public void CreateDirectory(FileSystemPath path)
		{
			using (var r = Refer(path))
			{
				r.FileSystem.CreateDirectory(GetRelativePath(path));
			}
		}

		public void Delete(FileSystemPath path)
		{
			using (var r = Refer(path))
			{
				r.FileSystem.Delete(GetRelativePath(path));
			}
		}

		#endregion
	}
}
