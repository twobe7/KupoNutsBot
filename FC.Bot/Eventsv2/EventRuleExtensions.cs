// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using FC.Eventsv2;
	using NodaTime;

	public static class EventRuleExtensions
	{
		public static List<Occurrence> GetOccurrences(this Event.Rule self, Event owner, Instant from, Instant to)
		{
			List<Occurrence> results = new List<Occurrence>();
			Instant starting = owner.GetInstant(self.StartTime);

			if (starting > from && starting < to)
				results.Add(new Occurrence(starting, self.Duration));

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

						if (instant < from)
							continue;

						results.Add(new Occurrence(instant, self.Duration));
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

						if (instant < from)
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

							results.Add(new Occurrence(newInstant, self.Duration));
						}
					}
					while (instant < to);

					break;
				}

				case Event.Rule.TimeUnit.Month:
				{
					throw new NotImplementedException();
				}

				case Event.Rule.TimeUnit.Year:
				{
					throw new NotImplementedException();
				}
			}

			return results;
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
