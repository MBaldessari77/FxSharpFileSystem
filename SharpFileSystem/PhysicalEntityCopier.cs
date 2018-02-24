using SharpFileSystem.FileSystems;

namespace SharpFileSystem
{
	// ReSharper disable once UnusedMember.Global
	public class PhysicalEntityCopier : IEntityCopier
	{
		public void Copy(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath)
		{
			var pSource = (PhysicalFileSystem) source;
			var pDestination = (PhysicalFileSystem) destination;
			var pSourcePath = pSource.GetPhysicalPath(sourcePath);
			var pDestinationPath = pDestination.GetPhysicalPath(destinationPath);
			if (sourcePath.IsFile)
			{
				System.IO.File.Copy(pSourcePath, pDestinationPath);
			}
			else
			{
				destination.CreateDirectory(destinationPath);
				foreach (var e in source.GetEntities(sourcePath))
					source.Copy(e, destination, e.IsFile ? destinationPath.AppendFile(e.EntityName) : destinationPath.AppendDirectory(e.EntityName));
			}
		}
	}
}
