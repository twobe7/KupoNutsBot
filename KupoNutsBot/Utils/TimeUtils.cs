// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class TimeUtils
	{
		public static string GetDateTimeString(DateTimeOffset dt)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(dt.ToString("dddd, dd MMMM"));
			builder.Append(GetTimeString(dt));
			return builder.ToString();
		}

		public static string GetTimeString(DateTimeOffset dt)
		{
			StringBuilder builder = new StringBuilder();
			TimeZoneInfo nzst = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
			builder.Append(dt.ToOffset(nzst.BaseUtcOffset).ToString("h:mm tt"));
			builder.Append(" nzst");
			builder.Append(" - ");
			TimeZoneInfo awst = TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time");
			builder.Append(dt.ToOffset(awst.BaseUtcOffset).ToString("h:mm tt"));
			builder.Append(" awst");
			builder.Append(" - ");
			TimeZoneInfo acst = TimeZoneInfo.FindSystemTimeZoneById("AUS Central Standard Time");
			builder.Append(dt.ToOffset(acst.BaseUtcOffset).ToString("h:mm tt"));
			builder.Append(" acst");
			builder.Append(" - ");
			TimeZoneInfo aest = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
			builder.Append(dt.ToOffset(aest.BaseUtcOffset).ToString("h:mm tt"));
			builder.Append(" aest");
			return builder.ToString();
		}

		public static string GetDurationString(TimeSpan time)
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
	}
}
