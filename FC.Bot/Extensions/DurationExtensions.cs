// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Extensions
{
	using System.Text;
	using NodaTime;

	public static class DurationExtensions
	{
		public static string ToDisplayString(this Duration self)
		{
			StringBuilder builder = new StringBuilder();

			bool comma = false;
			builder.Append(self.Days, "Day", "Days", ref comma);
			builder.Append(self.Hours, "Hour", "Hours", ref comma);
			builder.Append(self.Minutes, "Minute", "Minutes", ref comma);

			return builder.ToString();
		}

		private static void Append(this StringBuilder self, int count, string singular, string multiple, ref bool comma)
		{
			if (count > 0)
			{
				if (comma)
					self.Append(", ");

				self.Append(count);
				self.Append(" ");

				if (count == 1)
				{
					self.Append(singular);
				}
				else if (count > 1)
				{
					self.Append(multiple);
				}

				comma = true;
			}
		}
	}
}
