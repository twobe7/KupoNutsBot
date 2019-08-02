// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using KupoNuts.Utils;
	using NodaTime;
	using NodaTime.Text;

	public static class EventExtensions
	{
		public static Duration GetDuration(this Event self)
		{
			if (string.IsNullOrEmpty(self.Duration))
				return Duration.FromSeconds(0);

			try
			{
				return DurationPattern.Roundtrip.Parse(self.Duration).Value;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to deserialize duration string: \"" + self.Duration + "\" " + ex.Message);
				return Duration.FromSeconds(0);
			}
		}

		public static void GetDuration(this Event self, out string duration)
		{
			Duration dur = self.GetDuration();
			duration = dur.Hours.ToString("D2") + ":" + dur.Minutes.ToString("D2");
		}

		public static void SetDuration(this Event self, string duration)
		{
			string[] parts = duration.Split(':');

			if (parts.Length != 2)
				throw new Exception("Invalid duration format: \"" + duration + "\"");

			int hour = int.Parse(parts[0]);
			int minute = int.Parse(parts[1]);

			Duration dur = Duration.FromMinutes((hour * 60) + minute);
			self.Duration = DurationPattern.Roundtrip.Format(dur);
		}

		public static Instant GetDateTime(this Event self)
		{
			if (string.IsNullOrEmpty(self.DateTime))
				return Instant.FromJulianDate(0);

			return InstantPattern.ExtendedIso.Parse(self.DateTime).Value;
		}

		public static void SetDateTime(this Event self, string date, string time)
		{
			string[] parts = date.Split('-');

			if (parts.Length != 3)
				throw new Exception("Invalid date format: \"" + date + "\"");

			int year = int.Parse(parts[0]);
			int month = int.Parse(parts[1]);
			int day = int.Parse(parts[2]);

			parts = time.Split(':');

			if (parts.Length != 2)
				throw new Exception("Invalid time format: \"" + time + "\"");

			int hour = int.Parse(parts[0]);
			int minute = int.Parse(parts[1]);

			LocalDateTime ldt = new LocalDateTime(year, month, day, hour, minute);
			ZonedDateTime zdt = ldt.InZoneLeniently(DateTimeZoneProviders.Tzdb.GetSystemDefault());
			Instant instant = zdt.ToInstant();

			self.SetDateTime(instant);
		}

		public static void GetDateTime(this Event self, out string date, out string time)
		{
			Instant instant = self.GetDateTime();

			ZonedDateTime zdt = instant.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault());
			LocalDateTime ldt = zdt.LocalDateTime;

			date = ldt.Year.ToString("D4") + "-" + ldt.Month.ToString("D2") + "-" + ldt.Day.ToString("D2");
			time = ldt.Hour.ToString("D2") + ":" + ldt.Minute.ToString("D2");
		}

		public static void SetDateTime(this Event self, Instant instant)
		{
			self.DateTime = InstantPattern.ExtendedIso.Format(instant);
		}

		public static string GetNextOccurance(this Event self)
		{
			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			Instant next = self.GetNextOccurance(zone);
			ZonedDateTime zdt = next.InZone(zone);
			return zdt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
		}

		public static Instant GetNextOccurance(this Event self, DateTimeZone zone)
		{
			Instant eventDateTime = self.GetDateTime();

			Instant now = SystemClock.Instance.GetCurrentInstant();
			if (eventDateTime < now && self.Repeats != 0)
			{
				LocalDate nextDate;
				LocalDateTime dateTime = eventDateTime.InZone(zone).LocalDateTime;

				LocalDate date = dateTime.Date;
				LocalDate todaysDate = TimeUtils.Now.InZone(zone).LocalDateTime.Date;

				List<LocalDate> dates = new List<LocalDate>();
				foreach (Event.Days day in Enum.GetValues(typeof(Event.Days)))
				{
					if (!FlagsUtils.IsSet(self.Repeats, day))
						continue;

					nextDate = todaysDate.Next(TimeUtils.ToIsoDay(day));
					dates.Add(nextDate);
				}

				if (dates.Count <= 0)
					return eventDateTime;

				dates.Sort();
				nextDate = dates[0];

				Period dateOffset = nextDate - date;
				dateTime = dateTime + dateOffset;

				Instant nextInstant = dateTime.InZoneLeniently(zone).ToInstant();
				return nextInstant;
			}

			return eventDateTime;
		}
	}
}
