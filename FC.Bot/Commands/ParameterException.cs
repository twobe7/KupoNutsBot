// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot.Commands
{
	using System;

	public class ParameterException : Exception
	{
		public ParameterException(string message)
			: base(message)
		{
		}
	}
}
