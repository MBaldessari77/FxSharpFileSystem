using System;
using System.Collections.Generic;
using System.IO;

namespace SharpFileSystem.FileSystems
{
	// ReSharper disable once UnusedMember.Global
	public class ReadOnlyFileSystem : IFileSystem
	{
		public ReadOnlyFileSystem(IFileSystem fileSystem) { FileSystem = fileSystem; }
		public void Dispose() { FileSystem.Dispose(); }

		// ReSharper disable once MemberCanBePrivate.Global
		public IFileSystem FileSystem { get; }
		public ICollection<FileSystemPath> GetEntities(FileSystemPath path) { return FileSystem.GetEntities(path); }
		public bool Exists(FileSystemPath path) { return FileSystem.Exists(path); }

		public Stream OpenFile(FileSystemPath path, FileAccess access)
		{
			if (access != FileAccess.Read)
				throw new InvalidOperationException("This is a read-only filesystem.");
			return FileSystem.OpenFile(path, access);
		}

		public Stream CreateFile(FileSystemPath path) { throw new InvalidOperationException("This is a read-only filesystem."); }
		public void CreateDirectory(FileSystemPath path) { throw new InvalidOperationException("This is a read-only filesystem."); }
		public void Delete(FileSystemPath path) { throw new InvalidOperationException("This is a read-only filesystem."); }
	}
}
