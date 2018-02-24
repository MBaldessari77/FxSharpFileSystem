using System.Collections.Generic;
using System.IO;

namespace SharpFileSystem.FileSystems
{
	// ReSharper disable once UnusedMember.Global
	public class FileSystemWrapper : IFileSystem
	{
		public FileSystemWrapper(IFileSystem parent) { Parent = parent; }
		public void Dispose() { Parent.Dispose(); }

		// ReSharper disable once MemberCanBePrivate.Global
		public IFileSystem Parent { get; }
		public ICollection<FileSystemPath> GetEntities(FileSystemPath path) { return Parent.GetEntities(path); }
		public bool Exists(FileSystemPath path) { return Parent.Exists(path); }
		public Stream CreateFile(FileSystemPath path) { return Parent.CreateFile(path); }
		public Stream OpenFile(FileSystemPath path, FileAccess access) { return Parent.OpenFile(path, access); }
		public void CreateDirectory(FileSystemPath path) { Parent.CreateDirectory(path); }
		public void Delete(FileSystemPath path) { Parent.Delete(path); }
	}
}
