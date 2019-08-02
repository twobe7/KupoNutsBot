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
	}
}
