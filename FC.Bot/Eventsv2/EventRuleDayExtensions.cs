// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using System.Collections.Generic;
	using System.Text;
	using FC.Eventsv2;

	public static class EventRuleDayExtensions
	{
		public static string ToDisplayString(this Event.Rule.Day self)
		{
			List<string> parts = new List<string>();
			Append(self, Event.Rule.Day.Monday, ref parts);
			Append(self, Event.Rule.Day.Tuesday, ref parts);
			Append(self, Event.Rule.Day.Wednesday, ref parts);
			Append(self, Event.Rule.Day.Thursday, ref parts);
			Append(self, Event.Rule.Day.Friday, ref parts);
			Append(self, Event.Rule.Day.Saturday, ref parts);
			Append(self, Event.Rule.Day.Sunday, ref parts);

			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < parts.Count; i++)
			{
				if (i > 1)
					builder.Append(", ");

				if (i == 1 && parts.Count == 2)
					builder.Append(" and ");

				if (i > 1 && i >= parts.Count - 1)
					builder.Append("and ");

				builder.Append(parts[i]);
			}

			return builder.ToString();
		}

		private static void Append(Event.Rule.Day mask, Event.Rule.Day day, ref List<string> parts)
		{
			if (!mask.HasFlag(day))
				return;

			parts.Add(day.ToShortString());
		}

		private static string ToShortString(this Event.Rule.Day day)
		{
			switch (day)
			{
				case Event.Rule.Day.Monday: return "Mon";
				case Event.Rule.Day.Tuesday: return "Tue";
				case Event.Rule.Day.Wednesday: return "Wed";
				case Event.Rule.Day.Thursday: return "Thur";
				case Event.Rule.Day.Friday: return "Fri";
				case Event.Rule.Day.Saturday: return "Sat";
				case Event.Rule.Day.Sunday: return "Sun";
			}

			return string.Empty;
		}
	}
}
