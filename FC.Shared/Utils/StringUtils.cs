﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;

	public class StringUtils
	{
		private static readonly Regex CompiledUnicodeRegex = new Regex(@"[^\u0000-\u007F]", RegexOptions.Compiled);

		public static int ComputeLevenshtein(string s, string t)
		{
			// Remove unicode and spaces
			s = CompiledUnicodeRegex.Replace(s.Replace(" ", string.Empty), string.Empty);
			t = CompiledUnicodeRegex.Replace(t.Replace(" ", string.Empty), string.Empty);

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Verify arguments.
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Initialize arrays.
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			// Begin looping.
			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					// Compute cost.
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					d[i, j] = Math.Min(
					Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
					d[i - 1, j - 1] + cost);
				}
			}

			// Return cost.
			return d[n, m];
		}
	}
}
