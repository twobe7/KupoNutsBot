// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using KupoNuts.Events;
	using NodaTime;
	using NodaTime.TimeZones;

	public static class TimeUtils
	{
		public static Instant Now
		{
			get
			{
				return SystemClock.Instance.GetCurrentInstant();
			}
		}

		public static DateTimeZone Aukland
		{
			get
			{
				return GetTimeZone("Pacific/Auckland");
			}
		}

		public static DateTimeZone Perth
		{
			get
			{
				return GetTimeZone("Australia/Perth");
			}
		}

		public static DateTimeZone Adelaide
		{
			get
			{
				return GetTimeZone("Australia/Adelaide");
			}
		}

		public static DateTimeZone Sydney
		{
			get
			{
				return GetTimeZone("Australia/Sydney");
			}
		}

		public static string GetDateTimeString(DateTimeOffset? dt)
		{
			if (dt == null)
				return string.Empty;

			return GetDateTimeString(Instant.FromDateTimeOffset((DateTimeOffset)dt));
		}

		public static string GetDateString(DateTimeOffset? dt)
		{
			if (dt == null)
				return string.Empty;

			return GetDateString(Instant.FromDateTimeOffset((DateTimeOffset)dt));
		}

		public static string GetDateTimeString(Instant? dt, DateTimeZone? tz = null)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(GetDateString(dt, tz));
			builder.Append(GetTimeString(dt, tz));
			return builder.ToString();
		}

		public static string GetDateString(Instant? dt, DateTimeZone? tz = null)
		{
			if (dt == null)
				return string.Empty;

			return GetDateString((Instant)dt, tz);
		}

		public static string GetDateString(Instant dt, DateTimeZone? tz = null)
		{
			if (tz == null)
				tz = Sydney;

			return dt.InZone(tz).ToString("dddd dd MMMM, yyyy", CultureInfo.InvariantCulture);
		}

		public static string GetTimeString(Instant? dt, DateTimeZone? tz = null)
		{
			if (dt == null)
				return string.Empty;

			if (tz == null)
			{
				return GetTimeString((Instant)dt);
			}
			else
			{
				return GetTimeString((Instant)dt, tz, tz.Id);
			}
		}

		public static string GetTimeString(Instant dt)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(GetTimeString(dt, Perth, IsStandardTime(dt, Perth) ? " AWST" : " AWDT"));
			builder.Append(" - ");
			builder.Append(GetTimeString(dt, Adelaide, IsStandardTime(dt, Adelaide) ? " ACST" : " ACDT"));
			builder.Append(" - ");
			builder.Append(GetTimeString(dt, Sydney, IsStandardTime(dt, Sydney) ? " AEST" : " AEDT"));
			builder.Append(" - ");
			builder.Append(GetTimeString(dt, Aukland, IsStandardTime(dt, Aukland) ? " NZST" : " NZDT"));
			return builder.ToString();
		}

		public static string GetTimeString(Instant dt, DateTimeZone tz, string zoneName)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(dt.InZone(tz).ToString("**h:mm", CultureInfo.InvariantCulture));
			builder.Append(dt.InZone(tz).ToString("tt**", CultureInfo.InvariantCulture).ToLower());
			builder.Append(" ");
			builder.Append(zoneName);
			return builder.ToString();
		}

		public static bool IsStandardTime(Instant instant, DateTimeZone zone)
		{
			ZoneInterval zoneInterval = zone.GetZoneInterval(instant);
			Offset offset = zoneInterval.Savings;
			if (offset.Seconds > 0)
				return false;

			return true;
		}

		public static string? GetDurationString(Duration? timeNull)
		{
			if (timeNull == null)
				return null;

			Duration time = (Duration)timeNull;
			StringBuilder builder = new StringBuilder();

			if (time.Days == 0 && time.Hours == 0 && time.Minutes == 0)
				return " now";

			if (time.Days == 1)
			{
				builder.Append(" ");
				builder.Append(time.Days);
				builder.Append(" day");
			}
			else if (time.Days > 1)
			{
				builder.Append(" ");
				builder.Append(time.Days);
				builder.Append(" days");
			}

			if (time.Hours == 1)
			{
				builder.Append(" ");
				builder.Append(time.Hours);
				builder.Append(" hour");
			}
			else if (time.Hours > 1)
			{
				builder.Append(" ");
				builder.Append(time.Hours);
				builder.Append(" hours");
			}

			if (time.Minutes == 1)
			{
				builder.Append(" ");
				builder.Append(time.Minutes);
				builder.Append(" minute");
			}
			else if (time.Minutes > 1)
			{
				builder.Append(" ");
				builder.Append(time.Minutes);
				builder.Append(" minutes");
			}

			return builder.ToString();
		}

		public static Instant RoundInstant(Instant instant)
		{
			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			ZonedDateTime zdt = instant.InZone(zone);

			instant += Duration.FromNanoseconds(-zdt.NanosecondOfSecond);
			instant += Duration.FromSeconds(-zdt.Second);

			int minute = zdt.Minute;
			int newMinute = (int)Math.Round(minute / 15.0) * 15;
			Duration minutechange = Duration.FromMinutes(newMinute - minute);
			instant += minutechange;

			return instant;
		}

		public static string GetDayName(int daysAway)
		{
			if (daysAway == 0)
				return "Today";

			if (daysAway == 1)
				return "Tomorrow";

			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			Instant then = TimeUtils.Now + Duration.FromDays(daysAway);

			if (daysAway >= 7)
			{
				return GetDateString(then);
			}
			else
			{
				IsoDayOfWeek day = then.InZone(zone).DayOfWeek;
				return day.ToString();
			}
		}

		public static int GetDaysTill(Instant instant, DateTimeZone zone)
		{
			ZonedDateTime zdt = TimeUtils.Now.InZone(zone);
			LocalDateTime ldt = zdt.LocalDateTime;
			ldt = ldt.Date.AtMidnight();
			zdt = ldt.InZoneLeniently(zone);

			Duration duration = instant - zdt.ToInstant();

			return (int)Math.Floor(duration.TotalDays);
		}

		public static string GetDurationString(double duration)
		{
			if (duration == 0)
				return "0 minutes";

			int hours = (int)duration;
			int minutes = (int)((duration - (double)hours) * 60.0);

			StringBuilder builder = new StringBuilder();

			if (hours == 1)
			{
				builder.Append(hours);
				builder.Append(" hour ");
			}
			else if (hours > 1)
			{
				builder.Append(hours);
				builder.Append(" hours ");
			}

			if (minutes == 1)
			{
				builder.Append(minutes);
				builder.Append(" minute ");
			}
			else if (minutes > 1)
			{
				builder.Append(minutes);
				builder.Append(" minutes ");
			}

			return builder.ToString();
		}

		public static DateTimeZone GetTimeZone(string id)
		{
			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(id);
			if (zone == null)
				throw new Exception("Failed to get time zone: \"" + id + "\"");

			return zone;
		}
	}
}
