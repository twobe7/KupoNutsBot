// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using KupoNuts.Events;
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

		public static IsoDayOfWeek ToIsoDay(Event.Days day)
		{
			switch (day)
			{
				case Event.Days.Monday: return IsoDayOfWeek.Monday;
				case Event.Days.Tuesday: return IsoDayOfWeek.Tuesday;
				case Event.Days.Wednesday: return IsoDayOfWeek.Wednesday;
				case Event.Days.Thursday: return IsoDayOfWeek.Thursday;
				case Event.Days.Friday: return IsoDayOfWeek.Friday;
				case Event.Days.Saturday: return IsoDayOfWeek.Saturday;
				case Event.Days.Sunday: return IsoDayOfWeek.Sunday;
			}

			throw new Exception("Unknown day: " + day);
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
	}
}
