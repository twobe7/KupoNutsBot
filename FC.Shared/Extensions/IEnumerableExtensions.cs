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

			#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
			return default;
			#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
		}
	}
}
