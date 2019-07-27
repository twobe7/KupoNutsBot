// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class TimeUtils
	{
		public static string GetTimeString(DateTimeOffset dt)
		{
			TimeZoneInfo aest = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
			TimeZoneInfo awst = TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time");
			TimeZoneInfo nzst = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");

			StringBuilder builder = new StringBuilder();
			builder.AppendLine(dt.ToString("dddd, dd MMMM"));
			builder.AppendLine(dt.ToOffset(aest.BaseUtcOffset).ToString("  H:mm AEST"));
			builder.AppendLine(dt.ToOffset(awst.BaseUtcOffset).ToString("  H:mm AWST"));
			builder.AppendLine(dt.ToOffset(nzst.BaseUtcOffset).ToString("  H:mm NZST"));
			return builder.ToString();
		}
	}
}
