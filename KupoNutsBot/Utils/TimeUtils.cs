// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using NodaTime;

	public static class TimeUtils
	{
		public static Instant Now
		{
			get
			{
				return SystemClock.Instance.GetCurrentInstant();
			}
		}

		private static DateTimeZone NZST
		{
			get
			{
				return GetTimeZone("Pacific/Auckland");
			}
		}

		private static DateTimeZone AWST
		{
			get
			{
				return GetTimeZone("Australia/Perth");
			}
		}

		private static DateTimeZone ACST
		{
			get
			{
				return GetTimeZone("Australia/Adelaide");
			}
		}

		private static DateTimeZone AEST
		{
			get
			{
				return GetTimeZone("Australia/Sydney");
			}
		}

		public static string GetDateTimeString(Instant dt)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(dt.InZone(AEST).ToString("dddd, dd MMMM", CultureInfo.InvariantCulture));
			builder.Append(GetTimeString(dt));
			return builder.ToString();
		}

		public static string GetTimeString(Instant dt)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(dt.InZone(AWST).ToString("h:mm tt", CultureInfo.InvariantCulture));
			builder.Append(" awst");
			builder.Append(" - ");
			builder.Append(dt.InZone(ACST).ToString("h:mm tt", CultureInfo.InvariantCulture));
			builder.Append(" acst");
			builder.Append(" - ");
			builder.Append(dt.InZone(AEST).ToString("h:mm tt", CultureInfo.InvariantCulture));
			builder.Append(" aest");
			builder.Append(" - ");
			builder.Append(dt.InZone(NZST).ToString("h:mm tt", CultureInfo.InvariantCulture));
			builder.Append(" nzst");
			return builder.ToString();
		}

		public static string GetDurationString(Duration time)
		{
			StringBuilder builder = new StringBuilder();

			if (time.Days == 1)
			{
				builder.Append(time.Days);
				builder.Append(" day ");
			}
			else if (time.Days > 1)
			{
				builder.Append(time.Days);
				builder.Append(" days ");
			}

			if (time.Hours == 1)
			{
				builder.Append(time.Hours);
				builder.Append(" hour ");
			}
			else if (time.Hours > 1)
			{
				builder.Append(time.Hours);
				builder.Append(" hours ");
			}

			if (time.Minutes == 1)
			{
				builder.Append(time.Minutes);
				builder.Append(" minute ");
			}
			else if (time.Minutes > 1)
			{
				builder.Append(time.Minutes);
				builder.Append(" minutes ");
			}

			return builder.ToString();
		}

		private static DateTimeZone GetTimeZone(string id)
		{
			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(id);
			if (zone == null)
				throw new Exception("Failed to get time zone: \"" + id + "\"");

			return zone;
		}
	}
}
