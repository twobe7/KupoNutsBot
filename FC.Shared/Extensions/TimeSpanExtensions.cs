// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace System
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class TimeSpanExtensions
	{
		/// <summary>
		/// Show timespan as formatted string
		/// With days, show as X days 00:00:00
		/// Without days, just show 00:00:00.
		/// </summary>
		/// <param name="value">Value of Timespan.</param>
		/// <returns>Formatted string.</returns>
		public static string ToShortString(this TimeSpan value)
		{
			string output = string.Empty;

			if (value.Days > 0)
				output += value.Days + "days ";

			output += PadValue(value.Hours) + ":";

			output += PadValue(value.Minutes) + ":";

			output += PadValue(value.Seconds);

			// Trim trailing colon if needed
			////if (output.EndsWith(":"))
			////	output = output.Substring(0, output.Length - 1);

			return output;
		}

		/// <summary>
		/// Show timespan as formatted string
		/// Formatted as 0d 0h 0m 0s.
		/// </summary>
		/// <param name="value">Value of Timespan.</param>
		/// <param name="showSeconds">Sets whether to show Seconds, defaults to True.</param>
		/// <returns>string.</returns>
		public static string ToMediumString(this TimeSpan value, bool showSeconds = true)
		{
			string output = string.Empty;

			if (value.Days > 0)
				output += value.Days + "d ";

			if (value.Hours > 0)
				output += PadValue(value.Hours) + "h ";

			if (value.Minutes > 0)
				output += PadValue(value.Minutes) + "m ";

			if (showSeconds && value.Seconds > 0)
				output += PadValue(value.Seconds) + "s";

			return output;
		}

		/// <summary>
		/// Returns timespan as X Days, Y Hours, Z Minutes, I Seconds.
		/// </summary>
		/// <param name="value">TimeSpan value.</param>
		/// <returns>string.</returns>
		public static string ToLongString(this TimeSpan value)
		{
			string output = string.Empty;

			if (value.Days > 0)
				output += value.Days + (value.Days == 1 ? " Day" : " Days ");

			if (value.Hours > 0)
				output += value.Hours + (value.Hours == 1 ? " Hour" : " Hours ");

			if (value.Minutes > 0)
				output += value.Minutes + (value.Minutes == 1 ? " Minute" : " Minutes ");

			if (value.Seconds > 0)
				output += value.Seconds + (value.Seconds == 1 ? " Second" : " Seconds");

			// Remove trailing whitespace
			output = output.Trim();

			return output;
		}

		private static string PadValue(int value)
		{
			return value.ToString().PadLeft(2, '0');
		}
	}
}
