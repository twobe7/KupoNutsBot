// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using FC.Bot.Extensions;
	using FC.Eventsv2;
	using FC.Utils;
	using NodaTime;

	public static class EventExtensions
	{
		/*public static Dictionary<Event.Rule, Instant> GetOccurrences(this Event self, Instant from, Instant to)
		{
			Dictionary<Instant, Event.Rule> results = new Dictionary<Instant, Event.Rule>();
			foreach (Event.Rule rule in self.Rules)
			{
				List<Instant> occurrences = rule.GetOccurrences(self, from, to);
				results.Add(rule, occurrences);
			}

			return results;
		}*/

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

		public static LocalTime GetLocalTime(this Event self, Instant instant)
		{
			ZonedDateTime zdt = instant.InZone(self.BaseTimeZone);
			return zdt.LocalDateTime.TimeOfDay;
		}

		public static async Task UpdateNotices(this Event self)
		{
			List<Notice> notices = await self.GetNotices();
			foreach (Notice notice in notices)
			{
				await notice.Update();
			}
		}

		public static async Task<List<Notice>> GetNotices(this Event self)
		{
			Instant end = TimeUtils.Now + self.NoticeDuration;

			HashSet<Notice> results = new HashSet<Notice>();
			foreach (Event.Rule rule in self.Rules)
			{
				List<Instant> occurrences = rule.GetOccurrences(self, TimeUtils.Now, end);
				foreach (Instant occurrence in occurrences)
				{
					Notice notice = await self.GetNotice(occurrence, rule);
					results.Add(notice);
				}
			}

			return new List<Notice>(results);
		}

		public static async Task<Notice> GetNotice(this Event self, Instant occurrence, Event.Rule rule)
		{
			foreach (Event.Notice notice in self.Notices)
			{
				if (notice.Start.IsApproximately(occurrence))
				{
					return new Notice(self, notice, rule);
				}
			}

			// no notice for this occurrence, create one.
			Event.Notice newNotice = new Event.Notice();
			newNotice.Start = occurrence;
			self.Notices.Add(newNotice);
			await EventsService.SaveEvent(self);

			return new Notice(self, newNotice, rule);
		}

		public static string GetRepeatString(this Event self)
		{
			StringBuilder builder = new StringBuilder();
			foreach (Event.Rule rule in self.Rules)
			{
				builder.AppendLine(rule.ToDisplayString(self));
			}

			return builder.ToString();
		}
	}
}
