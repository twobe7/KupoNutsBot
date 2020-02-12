// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

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
