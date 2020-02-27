// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using FC.Bot.Extensions;
	using FC.Eventsv2;
	using FC.Utils;
	using NodaTime;

	public static class EventRuleExtensions
	{
		public static List<Instant> GetOccurrences(this Event.Rule self, Event owner, Instant from, Instant to)
		{
			List<Instant> results = new List<Instant>();
			Instant starting = owner.GetInstant(self.StartTime);

			if (starting > from && starting < to)
				results.Add(starting);

			if (self.RepeatEvery < 1)
				throw new Exception("Repeat must be greater than 0!");

			switch (self.Units)
			{
				case Event.Rule.TimeUnit.Day:
				{
					Instant instant = starting;
					do
					{
						instant = instant.Plus(Duration.FromDays(self.RepeatEvery));

						if (instant < from || instant > to)
							continue;

						results.Add(instant);
					}
					while (instant < to);

					break;
				}

				case Event.Rule.TimeUnit.Week:
				{
					Instant instant = starting;

					do
					{
						instant = instant.Plus(Duration.FromDays(7 * self.RepeatEvery));

						if (instant < from || instant > to)
							continue;

						ZonedDateTime zdt = instant.InZone(owner.BaseTimeZone);
						IsoDayOfWeek startDay = zdt.DayOfWeek;

						for (int i = 0; i < 7; i++)
						{
							IsoDayOfWeek day = startDay + i;

							if (day > IsoDayOfWeek.Sunday)
								day -= 7;

							if (!self.HasDay(day))
								continue;

							Instant newInstant = instant.Plus(Duration.FromDays(i));

							if (newInstant > to)
								continue;

							results.Add(newInstant);
						}
					}
					while (instant < to);

					break;
				}
			}

			return results;
		}

		public static string ToDisplayString(this Event.Rule self, Event owner)
		{
			LocalTime endTime = owner.GetLocalTime(owner.GetInstant(self.StartTime).Plus(self.Duration));
			StringBuilder builder = new StringBuilder();
			builder.Append(self.StartTime.ToDisplayString());
			builder.Append(" to ");
			builder.Append(endTime.ToDisplayString());
			builder.Append(" every ");

			if (self.RepeatEvery > 1)
			{
				builder.Append(self.RepeatEvery);
				builder.Append(NumberUtils.GetOrdinal(self.RepeatEvery));
				builder.Append(" ");
			}

			builder.Append(self.Units.ToString().ToLower());

			switch (self.Units)
			{
				case Event.Rule.TimeUnit.Week:
				{
					builder.Append(" on ");
					builder.Append(self.Days.ToDisplayString());
					break;
				}
			}

			builder.Append(".");

			return builder.ToString();
		}

		private static bool HasDay(this Event.Rule self, IsoDayOfWeek day)
		{
			return self.Days.HasFlag(GetDay(day));
		}

		private static Event.Rule.Day GetDay(IsoDayOfWeek day)
		{
			switch (day)
			{
				case IsoDayOfWeek.Monday: return Event.Rule.Day.Monday;
				case IsoDayOfWeek.Tuesday: return Event.Rule.Day.Tuesday;
				case IsoDayOfWeek.Wednesday: return Event.Rule.Day.Wednesday;
				case IsoDayOfWeek.Thursday: return Event.Rule.Day.Thursday;
				case IsoDayOfWeek.Friday: return Event.Rule.Day.Friday;
				case IsoDayOfWeek.Saturday: return Event.Rule.Day.Saturday;
				case IsoDayOfWeek.Sunday: return Event.Rule.Day.Sunday;
			}

			throw new Exception("unrecognized day of week: " + day);
		}
	}
}
