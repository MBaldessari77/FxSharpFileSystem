using System.Collections.Generic;

namespace SharpFileSystem
{
	// ReSharper disable once UnusedMember.Global
	public static class Extensions
	{
		// ReSharper disable once UnusedMember.Global
		public static void Add<TKey, TValue>(ICollection<KeyValuePair<TKey, TValue>> collection, TKey key, TValue value) { collection.Add(new KeyValuePair<TKey, TValue>(key, value)); }
	}
}
