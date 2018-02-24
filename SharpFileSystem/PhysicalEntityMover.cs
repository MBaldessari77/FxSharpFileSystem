using SharpFileSystem.FileSystems;

namespace SharpFileSystem
{
	public class PhysicalEntityMover : IEntityMover
	{
		public void Move(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath)
		{
			var pSource = (PhysicalFileSystem) source;
			var pDestination = (PhysicalFileSystem) destination;
			var pSourcePath = pSource.GetPhysicalPath(sourcePath);
			var pDestinationPath = pDestination.GetPhysicalPath(destinationPath);
			if (sourcePath.IsFile)
				System.IO.File.Move(pSourcePath, pDestinationPath);
			else
				System.IO.Directory.Move(pSourcePath, pDestinationPath);
		}
	}
}
