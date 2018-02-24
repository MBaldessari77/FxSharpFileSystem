using System.IO;

namespace SharpFileSystem.IO
{
	public static class StreamExtensions
	{
		const int DefaultBufferSize = 4096;

		// ReSharper disable once UnusedMember.Global
		public static string ReadAllText(this Stream s)
		{
			using (var reader = new StreamReader(s))
			{
				return reader.ReadToEnd();
			}
		}

		static void StreamTo(this Stream s, Stream destination, int bufferSize = DefaultBufferSize)
		{
			var b = new byte[bufferSize];
			int readBytes;
			while ((readBytes = s.Read(b, 0, bufferSize)) > 0)
				destination.Write(b, 0, readBytes);
		}

		// ReSharper disable once UnusedMember.Global
		public static byte[] Read(this Stream s, int count)
		{
			var buffer = new byte[count];
			s.Read(buffer, 0, count);
			return buffer;
		}

		public static void Write(this Stream s, byte[] buffer) { s.Write(buffer, 0, buffer.Length); }

		// ReSharper disable once UnusedMember.Global
		public static void ReadAll(this Stream s) { s.StreamTo(EmptyStream.Instance); }

		public static byte[] ReadAllBytes(this Stream s)
		{
			using (var ms = new MemoryStream())
			{
				StreamTo(s, ms);
				return ms.ToArray();
			}
		}
	}
}
