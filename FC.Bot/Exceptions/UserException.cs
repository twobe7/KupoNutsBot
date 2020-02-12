// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

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
