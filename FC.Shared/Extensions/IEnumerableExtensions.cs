// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace System.Collections.Generic
{
	public static class IEnumerableExtensions
	{
		public static T Getfirst<T>(this IEnumerable<T> self)
		{
			foreach (T entry in self)
			{
				return entry;
			}

			throw new Exception("Enumerable is empty");
		}
	}
}
