// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Bot.Extensions;
	using FC.Eventsv2;
	using FC.Utils;
	using NodaTime;

	public static class EventExtensions
	{
		/// <summary>
		/// Gets all instances of this event between the given instants.
		/// </summary>
		public static List<Occurrence> GetOccurrences(this Event self, Instant from, Instant to)
		{
			List<Occurrence> results = new List<Occurrence>();
			foreach (Event.Rule rule in self.Rules)
			{
				results.AddRange(rule.GetOccurrences(self, from, to));
			}

			return results;
		}

		public static Instant GetInstant(this Event self, LocalTime time)
		{
			if (self.BeginDate == null)
				throw new Exception("Non repeating event missing begin date");

			if (self.BaseTimeZone == null)
				throw new Exception("Non repeating event missing base time zone");

			LocalDateTime ldt = (LocalDate)self.BeginDate + time;
			ZonedDateTime zdt = ldt.InZoneLeniently(self.BaseTimeZone);
			return zdt.ToInstant();
		}

		public static async Task UpdateNotices(this Event self)
		{
			List<Event.Notice> notices = await self.GetNotices();
			foreach (Event.Notice notice in notices)
			{
				await notice.Update(self);
			}
		}

		public static async Task<List<Event.Notice>> GetNotices(this Event self)
		{
			Instant end = TimeUtils.Now + self.NoticeDuration;
			List<Occurrence> occurrences = self.GetOccurrences(TimeUtils.Now, end);

			List<Event.Notice> results = new List<Event.Notice>();
			foreach (Occurrence occurrence in occurrences)
			{
				results.Add(await self.GetNotice(occurrence));
			}

			return results;
		}

		public static async Task<Event.Notice> GetNotice(this Event self, Occurrence occurrence)
		{
			foreach (Event.Notice notice in self.Notices)
			{
				if (notice.Start.IsApproximately(occurrence.Instant))
				{
					return notice;
				}
			}

			// no notice for this occurrence, create one.
			Event.Notice newNotice = new Event.Notice();
			newNotice.Start = occurrence.Instant;
			self.Notices.Add(newNotice);
			await EventsService.SaveEvent(self);

			return newNotice;
		}
	}
}
