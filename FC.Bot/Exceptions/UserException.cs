// This document is intended for use by Kupo Nut Brigade developers.

namespace FC
{
	using System;

	public class UserException : Exception
	{
		public UserException(string message)
			: base(message)
		{
		}
	}
}
