// This document is intended for use by Kupo Nut Brigade developers.

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
