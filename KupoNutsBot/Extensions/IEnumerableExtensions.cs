// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class IEnumerableExtensions
	{
		public static T Getfirst<T>(this IEnumerable<T> self)
		{
			foreach (T entry in self)
			{
				return entry;
			}

			return default(T);
		}
	}
}
