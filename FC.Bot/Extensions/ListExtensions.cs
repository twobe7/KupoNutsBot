// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace System.Collections.Generic
{
	using System;

	public static class ListExtensions
	{
		public static T GetRandom<T>(this List<T> self)
		{
			if (self == null || self.Count <= 0)
				throw new Exception("List has no items");

			Random rn = new Random();
			return self[rn.Next(self.Count)];
		}
	}
}
