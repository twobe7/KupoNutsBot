// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Extensions
{
	using System.Text;
	using NodaTime;

	public static class DurationExtensions
	{
		public static string ToDisplayString(this Duration self)
		{
			StringBuilder builder = new StringBuilder();

			if (self.TotalMinutes == 0)
				return " now";

			Append(self.Days, builder, "day", "days");
			Append(self.Hours, builder, "hour", "hours");
			Append(self.Minutes, builder, "minute", "minutes");

			return builder.ToString();
		}

		private static void Append(int value, StringBuilder builder, string name, string namePlural)
		{
			if (value == 0)
				return;

			builder.Append(" ");
			builder.Append(value);
			builder.Append(" ");
			builder.Append(value == 1 ? name : namePlural);
		}
	}
}
