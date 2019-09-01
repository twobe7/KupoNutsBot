// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using KupoNuts.Events;
	using KupoNuts.Utils;
	using NodaTime;
	using NodaTime.Text;

	public static class EventExtensions
	{
		public static ulong GetServerId(this Event self)
		{
			return ulong.Parse(self.ServerId);
		}

		public static string? GetRepeatsString(this Event self)
		{
			if (self.Repeats == Event.Days.None)
				return null;

			StringBuilder builder = new StringBuilder();
			builder.Append("Every ");

			int count = 0;
			foreach (Event.Days day in DaysUtil.AllDays)
			{
				if (FlagsUtils.IsSet(self.Repeats, day))
				{
					if (count > 0)
						builder.Append(", ");

					count++;
					builder.Append(day);
				}
			}

			if (count == 7)
			{
				builder.Clear();
				builder.Append("Every day");
			}

			return builder.ToString();
		}

		public static Duration? GetNotifyDuration(this Event self)
		{
			if (string.IsNullOrEmpty(self.NotifyDuration))
				return null;

			return DurationPattern.Roundtrip.Parse(self.NotifyDuration).Value;
		}

		public static Duration GetDuration(this Event self)
		{
			if (string.IsNullOrEmpty(self.Duration))
				return Duration.FromSeconds(0);

			return DurationPattern.Roundtrip.Parse(self.Duration).Value;
		}

		public static string GetWhenString(this Event self)
		{
			Duration? durationTill = self.GetDurationTill();

			if (durationTill == null)
				return "Never";

			Duration time = (Duration)durationTill;
			string str = "Starting ";

			if (time.TotalSeconds < 0)
			{
				Instant now = TimeUtils.RoundInstant(TimeUtils.Now);
				Instant instant = now + time + self.GetDuration();
				Duration endsIn = instant - now;

				time = endsIn;
				str = "Ending ";
				////return "Ends in" + TimeUtils.GetDurationString(endsIn);
			}

			string? endsInStr = TimeUtils.GetDurationString(time);
			if (endsInStr == null)
				return "Unknown";

			if (endsInStr.Contains("now"))
				return str + "now";

			return str + "in" + endsInStr;
		}

		public static Instant GetDateTime(this Event self)
		{
			if (string.IsNullOrEmpty(self.DateTime))
				return Instant.FromJulianDate(0);

			return InstantPattern.ExtendedIso.Parse(self.DateTime).Value;
		}

		public static Duration? GetDurationTill(this Event self)
		{
			Duration? duration = self.GetNextOccurance() - TimeUtils.RoundInstant(TimeUtils.Now);
			return duration;
		}

		public static List<Instant> GetNextOccurances(this Event self)
		{
			Instant eventDateTime = self.GetDateTime();
			Duration eventDuration = self.GetDuration();
			Instant now = SystemClock.Instance.GetCurrentInstant();

			List<Instant> occurances = new List<Instant>();

			if (self.Repeats == 0)
			{
				if (eventDateTime > now)
				{
					occurances.Add(eventDateTime);
				}
			}
			else
			{
				LocalDateTime dateTime = eventDateTime.InUtc().LocalDateTime;

				LocalDate date = dateTime.Date;
				LocalDate todaysDate = TimeUtils.Now.InUtc().LocalDateTime.Date;

				List<LocalDate> dates = new List<LocalDate>();
				foreach (Event.Days day in DaysUtil.AllDays)
				{
					if (!FlagsUtils.IsSet(self.Repeats, day))
						continue;

					IsoDayOfWeek dayOfWeek = TimeUtils.ToIsoDay(day);

					if (todaysDate.DayOfWeek == dayOfWeek)
					{
						Period datePeriod = Period.Between(dateTime.Date, todaysDate);
						LocalDateTime nextDateTime = dateTime + datePeriod;
						Instant occurance = nextDateTime.InUtc().ToInstant();

						if (occurance > now)
						{
							dates.Add(todaysDate);
							continue;
						}
					}

					dates.Add(todaysDate.Next(dayOfWeek));
				}

				dates.Sort();

				foreach (LocalDate nextDate in dates)
				{
					Period datePeriod = Period.Between(dateTime.Date, nextDate);

					LocalDateTime nextDateTime = dateTime + datePeriod;
					Instant occurance = nextDateTime.InUtc().ToInstant();

					if (occurance + eventDuration < now)
						continue;

					occurances.Add(occurance);
				}
			}

			return occurances;
		}

		public static Instant? GetNextOccurance(this Event self)
		{
			List<Instant> occurances = self.GetNextOccurances();
			if (occurances == null || occurances.Count <= 0)
				return null;

			return occurances[0];
		}
	}
}
