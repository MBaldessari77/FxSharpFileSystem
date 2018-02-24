using System;

namespace SharpFileSystem
{
	public abstract class FileSystemEntity : IEquatable<FileSystemEntity>
	{
		protected FileSystemEntity(IFileSystem fileSystem, FileSystemPath path)
		{
			FileSystem = fileSystem;
			Path = path;
		}

		public IFileSystem FileSystem { get; }
		public FileSystemPath Path { get; }
		public string Name => Path.EntityName;

		bool IEquatable<FileSystemEntity>.Equals(FileSystemEntity other) { return FileSystem.Equals(other.FileSystem) && Path.Equals(other.Path); }

		public override bool Equals(object obj) { return obj is FileSystemEntity other && ((IEquatable<FileSystemEntity>) this).Equals(other); }

		public override int GetHashCode() { return FileSystem.GetHashCode() ^ Path.GetHashCode(); }

		public static FileSystemEntity Create(IFileSystem fileSystem, FileSystemPath path) { return path.IsFile ? (FileSystemEntity) new File(fileSystem, path) : new Directory(fileSystem, path); }
	}
}
