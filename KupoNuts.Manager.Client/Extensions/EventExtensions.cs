// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using KupoNuts.Utils;
	using NodaTime;
	using NodaTime.Text;

	public static class EventExtensions
	{
		public static void GetNotifyDuration(this Event self, out double duration)
		{
			duration = 0;

			if (string.IsNullOrEmpty(self.NotifyDuration))
			{
				duration = -1;
				return;
			}

			Duration? dur = self.GetNotifyDuration();
			if (dur == null)
				return;

			duration = (dur.Value.Days * 24) + dur.Value.Hours + (dur.Value.Minutes / 60.0);
		}

		public static void SetNotifyDuration(this Event self, double duration)
		{
			if (duration < 0)
			{
				self.NotifyDuration = null;
				return;
			}

			int hours = (int)duration;
			int minutes = (int)((duration - (double)hours) * 60.0);

			Duration dur = Duration.FromMinutes((hours * 60) + minutes);
			self.NotifyDuration = DurationPattern.Roundtrip.Format(dur);
		}

		public static void GetDuration(this Event self, out double duration)
		{
			Duration dur = self.GetDuration();
			duration = dur.Hours + (dur.Minutes / 60.0);
		}

		public static void SetDuration(this Event self, double duration)
		{
			int hours = (int)duration;
			int minutes = (int)((duration - (double)hours) * 60.0);

			Duration dur = Duration.FromMinutes((hours * 60) + minutes);
			self.Duration = DurationPattern.Roundtrip.Format(dur);
		}

		public static void SetDateTime(this Event self, DateTime date, string time)
		{
			string[] parts = time.Split(':');

			if (parts.Length != 2)
				throw new Exception("Invalid time format: \"" + time + "\"");

			int hour = int.Parse(parts[0]);
			int minute = int.Parse(parts[1]);

			LocalDateTime ldt = new LocalDateTime(date.Year, date.Month, date.Day, hour, minute);
			ZonedDateTime zdt = ldt.InZoneLeniently(DateTimeZoneProviders.Tzdb.GetSystemDefault());
			Instant instant = zdt.ToInstant();

			self.SetDateTime(instant);
		}

		public static void GetDateTime(this Event self, out DateTime date, out string time)
		{
			Instant instant = self.GetDateTime();

			ZonedDateTime zdt = instant.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault());
			LocalDateTime ldt = zdt.LocalDateTime;

			////date = ldt.Year.ToString("D4") + "-" + ldt.Month.ToString("D2") + "-" + ldt.Day.ToString("D2");
			date = ldt.ToDateTimeUnspecified();
			time = ldt.Hour.ToString("D2") + ":" + ldt.Minute.ToString("D2");
		}

		public static void SetDateTime(this Event self, Instant instant)
		{
			self.DateTime = InstantPattern.ExtendedIso.Format(instant);
		}

		public static void GetRepeats(this Event self, out bool mon, out bool tue, out bool wed, out bool thu, out bool fri, out bool sat, out bool sun)
		{
			mon = FlagsUtils.IsSet(Event.Days.Monday, self.Repeats);
			tue = FlagsUtils.IsSet(Event.Days.Tuesday, self.Repeats);
			wed = FlagsUtils.IsSet(Event.Days.Wednesday, self.Repeats);
			thu = FlagsUtils.IsSet(Event.Days.Thursday, self.Repeats);
			fri = FlagsUtils.IsSet(Event.Days.Friday, self.Repeats);
			sat = FlagsUtils.IsSet(Event.Days.Saturday, self.Repeats);
			sun = FlagsUtils.IsSet(Event.Days.Sunday, self.Repeats);
		}

		public static void SetRepeats(this Event self, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat, bool sun)
		{
			Event.Days repeats = self.Repeats;
			FlagsUtils.Set(ref repeats, Event.Days.Monday, mon);
			FlagsUtils.Set(ref repeats, Event.Days.Tuesday, tue);
			FlagsUtils.Set(ref repeats, Event.Days.Wednesday, wed);
			FlagsUtils.Set(ref repeats, Event.Days.Thursday, thu);
			FlagsUtils.Set(ref repeats, Event.Days.Friday, fri);
			FlagsUtils.Set(ref repeats, Event.Days.Saturday, sat);
			FlagsUtils.Set(ref repeats, Event.Days.Sunday, sun);
			self.Repeats = repeats;
		}

		public static string GetNextOccuranceString(this Event self)
		{
			Instant? next = self.GetNextOccurance();

			if (next == null)
				return "Never";

			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			ZonedDateTime zdt = ((Instant)next).InZone(zone);
			StringBuilder builder = new StringBuilder();
			builder.Append(zdt.ToString("hh:mm ", CultureInfo.InvariantCulture));
			builder.Append(zdt.ToString("tt", CultureInfo.InvariantCulture).ToLower());
			builder.Append(zdt.ToString(" dd/MM/yyyy", CultureInfo.InvariantCulture).ToLower());
			return builder.ToString();
		}

		public static string GetChannelName(this Event self, List<Channel> channels)
		{
			if (string.IsNullOrEmpty(self.ChannelId))
				return null;

			foreach (Channel channel in channels)
			{
				if (channel.Id == self.ChannelId)
				{
					return channel.Name;
				}
			}

			return "Unknown";
		}
	}
}
