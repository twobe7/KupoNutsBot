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
			builder.Append(dt.ToOffset(nzst.BaseUtcOffset).ToString("  H:mm NZST"));
			builder.Append(" / ");
			TimeZoneInfo awst = TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time");
			builder.Append(dt.ToOffset(awst.BaseUtcOffset).ToString("  H:mm AWST"));
			builder.Append(" / ");
			TimeZoneInfo acst = TimeZoneInfo.FindSystemTimeZoneById("AUS Central Standard Time");
			builder.Append(dt.ToOffset(acst.BaseUtcOffset).ToString("  H:mm ACST"));
			builder.Append(" / ");
			TimeZoneInfo aest = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
			builder.Append(dt.ToOffset(aest.BaseUtcOffset).ToString("  H:mm AEST"));
			return builder.ToString();
		}

		public static string GetDurationString(TimeSpan time)
		{
			StringBuilder builder = new StringBuilder();

			if (time.Hours > 0)
			{
				builder.Append(time.Hours);
				builder.Append(" hours");
			}

			if (time.Hours > 0 && time.Minutes > 0)
				builder.Append(", ");

			if (time.Minutes > 0)
			{
				builder.Append(time.Minutes);
				builder.Append(" minutes");
			}

			return builder.ToString();
		}
	}
}
