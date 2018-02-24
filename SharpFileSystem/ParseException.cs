using System;

namespace SharpFileSystem
{
	public class ParseException : Exception
	{
		// ReSharper disable once UnusedMember.Global
		public ParseException(string input)
			: base("Could not parse input \"" + input + "\"")
		{
			Input = input;
			Reason = null;
		}

		public ParseException(string input, string reason)
			: base("Could not parse input \"" + input + "\": " + reason)
		{
			Input = input;
			Reason = reason;
		}

		// ReSharper disable UnusedAutoPropertyAccessor.Global
		// ReSharper disable MemberCanBePrivate.Global
		public string Input { get; }
		public string Reason { get; }
		// ReSharper restore MemberCanBePrivate.Global
		// ReSharper restore UnusedAutoPropertyAccessor.Global
	}
}
