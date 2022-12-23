// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace System
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class StringExtensions
	{
		public static string Truncate(this string value, int maxChars)
		{
			return value.Length <= maxChars ? value : value[..maxChars] + "...";
		}

		public static string RemoveLineBreaks(this string value)
		{
			return value.Replace("\n", " ").Replace("\r", " ");
		}
	}
}
