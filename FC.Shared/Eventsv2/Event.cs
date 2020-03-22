// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Eventsv2
{
	using System;
	using System.Collections.Generic;
	using FC.Attributes;
	using NodaTime;

	[Serializable]
	public class Event : EntryBase
	{
		public ulong GuildId { get; set; }
		public DateTimeZone? BaseTimeZone { get; set; }
		public LocalDate? BeginDate { get; set; }
		public Duration NoticeDuration { get; set; } = Duration.FromHours(24);
		public List<Rule> Rules { get; set; } = new List<Rule>();
		public List<Notice> Notices { get; set; } = new List<Notice>();

		[InspectorChannel]
		public ulong Channel { get; set; }

		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? ImageUrl { get; set; }

		[Serializable]
		public class Rule
		{
			public enum TimeUnit
			{
				Day,
				Week,
			}

			[Flags]
			public enum Day
			{
				None = 0,
				Monday = 1,
				Tuesday = 2,
				Wednesday = 4,
				Thursday = 8,
				Friday = 16,
				Saturday = 32,
				Sunday = 64,
			}

			public enum MonthMode
			{
				// the 18th of every month
				DayNumber,

				// the first Monday of every month
				FirstDay,
				SecondDay,
				ThirdDay,
				FourthDay,
			}

			public int RepeatEvery { get; set; } = 0;
			public TimeUnit Units { get; set; } = TimeUnit.Week;
			public Day Days { get; set; } = Day.None;
			public MonthMode Monthly { get; set; }
			public LocalTime StartTime { get; set; }
			public Duration Duration { get; set; }
		}

		[Serializable]
		public class Notice
		{
			public ulong? MessageId { get; set; }
			public Instant Start { get; set; }
		}
	}
}
