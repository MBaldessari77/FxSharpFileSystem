using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpFileSystem.Collections;

namespace SharpFileSystem
{
	public static class FileSystemExtensions
	{
		// ReSharper disable once UnusedMember.Global
		public static Stream Open(this File file, FileAccess access) { return file.FileSystem.OpenFile(file.Path, access); }

		// ReSharper disable once UnusedMember.Global
		public static void Delete(this FileSystemEntity entity) { entity.FileSystem.Delete(entity.Path); }

		// ReSharper disable once MemberCanBePrivate.Global
		public static ICollection<FileSystemPath> GetEntityPaths(this Directory directory) { return directory.FileSystem.GetEntities(directory.Path); }

		// ReSharper disable once UnusedMember.Global
		public static ICollection<FileSystemEntity> GetEntities(this Directory directory)
		{
			var paths = directory.GetEntityPaths();
			return new EnumerableCollection<FileSystemEntity>(paths.Select(p => FileSystemEntity.Create(directory.FileSystem, p)), paths.Count);
		}

		// ReSharper disable once UnusedMember.Global
		public static IEnumerable<FileSystemPath> GetEntitiesRecursive(this IFileSystem fileSystem, FileSystemPath path)
		{
			if (!path.IsDirectory)
				throw new ArgumentException("The specified path is not a directory.");
			foreach (var entity in fileSystem.GetEntities(path))
			{
				yield return entity;
				if (entity.IsDirectory)
					foreach (var subentity in fileSystem.GetEntitiesRecursive(entity))
						yield return subentity;
			}
		}

		// ReSharper disable once UnusedMember.Global
		public static void CreateDirectoryRecursive(this IFileSystem fileSystem, FileSystemPath path)
		{
			if (!path.IsDirectory)
				throw new ArgumentException("The specified path is not a directory.");
			var currentDirectoryPath = FileSystemPath.Root;
			foreach (var dirName in path.GetDirectorySegments())
			{
				currentDirectoryPath = currentDirectoryPath.AppendDirectory(dirName);
				if (!fileSystem.Exists(currentDirectoryPath))
					fileSystem.CreateDirectory(currentDirectoryPath);
			}
		}

		#region Move Extensions

		// ReSharper disable once MemberCanBePrivate.Global
		public static void Move(this IFileSystem sourceFileSystem, FileSystemPath sourcePath, IFileSystem destinationFileSystem, FileSystemPath destinationPath)
		{
			if (!EntityMovers.Registration.TryGetSupported(sourceFileSystem.GetType(), destinationFileSystem.GetType(), out var mover))
				throw new ArgumentException("The specified combination of file-systems is not supported.");
			mover.Move(sourceFileSystem, sourcePath, destinationFileSystem, destinationPath);
		}

		// ReSharper disable once UnusedMember.Global
		public static void MoveTo(this FileSystemEntity entity, IFileSystem destinationFileSystem, FileSystemPath destinationPath) { entity.FileSystem.Move(entity.Path, destinationFileSystem, destinationPath); }

		// ReSharper disable once UnusedMember.Global
		public static void MoveTo(this Directory source, Directory destination) { source.FileSystem.Move(source.Path, destination.FileSystem, destination.Path.AppendDirectory(source.Path.EntityName)); }

		// ReSharper disable once UnusedMember.Global
		public static void MoveTo(this File source, Directory destination) { source.FileSystem.Move(source.Path, destination.FileSystem, destination.Path.AppendFile(source.Path.EntityName)); }

		#endregion

		#region Copy Extensions

		public static void Copy(this IFileSystem sourceFileSystem, FileSystemPath sourcePath, IFileSystem destinationFileSystem, FileSystemPath destinationPath)
		{
			if (!EntityCopiers.Registration.TryGetSupported(sourceFileSystem.GetType(), destinationFileSystem.GetType(), out var copier))
				throw new ArgumentException("The specified combination of file-systems is not supported.");
			copier.Copy(sourceFileSystem, sourcePath, destinationFileSystem, destinationPath);
		}

		// ReSharper disable once UnusedMember.Global
		public static void CopyTo(this FileSystemEntity entity, IFileSystem destinationFileSystem, FileSystemPath destinationPath) { entity.FileSystem.Copy(entity.Path, destinationFileSystem, destinationPath); }

		// ReSharper disable once UnusedMember.Global
		public static void CopyTo(this Directory source, Directory destination) { source.FileSystem.Copy(source.Path, destination.FileSystem, destination.Path.AppendDirectory(source.Path.EntityName)); }

		// ReSharper disable once UnusedMember.Global
		public static void CopyTo(this File source, Directory destination) { source.FileSystem.Copy(source.Path, destination.FileSystem, destination.Path.AppendFile(source.Path.EntityName)); }

		#endregion
	}
}
