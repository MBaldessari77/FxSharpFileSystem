using System;
using System.Collections.Generic;

namespace SharpFileSystem
{
	public interface ITypeDictionary<T> : ICollection<T>
	{
		#region Indexers

		// ReSharper disable once UnusedMember.Global
		IEnumerable<T> this[Type type] { get; }

		#endregion

		#region Query Methods

		// ReSharper disable UnusedMemberInSuper.Global
		IEnumerable<T> Get(Type type);
		IEnumerable<T> GetExplicit(Type type);
		T GetSingle(Type type);
		T GetSingleExplicit(Type type);
		bool Contains(Type type);
		// ReSharper restore UnusedMemberInSuper.Global

		#endregion
	}
}
