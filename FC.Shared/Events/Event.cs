// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text.Json.Serialization;
	using Amazon.DynamoDBv2.DataModel;
	using FC.Utils;
	using NodaTime;
	using NodaTime.Text;

	[Serializable]
	public class Event : EntryBase
	{
		public enum Colors
		{
			Default,
			DarkerGrey,
			DarkGrey,
			LighterGrey,
			DarkRed,
			Red,
			DarkOrange,
			Orange,
			LightOrange,
			Gold,
			LightGrey,
			Magenta,
			DarkPurple,
			Purple,
			DarkBlue,
			Blue,
			DarkGreen,
			Green,
			DarkTeal,
			Teal,
			DarkMagenta,
		}

		public enum Statuses
		{
			Attending,
			Roles,
		}

		public string ServerIdStr { get; set; } = "0";
		public string? ChannelId { get; set; }
		public string Name { get; set; } = "New Event";
		public string? ShortDescription { get; set; }
		public string? Description { get; set; }
		public string? Message { get; set; }
		public string? Image { get; set; }
		public Colors Color { get; set; }
		public Statuses StatusType { get; set; }

		public Occurance BaseOccurance { get; set; } = new Occurance();
		public Occurance? Monday { get; set; }
		public Occurance? Tuesday { get; set; }
		public Occurance? Wednesday { get; set; }
		public Occurance? Thursday { get; set; }
		public Occurance? Friday { get; set; }
		public Occurance? Saturday { get; set; }
		public Occurance? Sunday { get; set; }

		public string? NotifyDurationStr { get; set; }
		public Notification? Notify { get; set; }

		public Duration? GetNotifyDuration()
		{
			if (string.IsNullOrEmpty(this.NotifyDurationStr))
				return null;

			return DurationPattern.Roundtrip.Parse(this.NotifyDurationStr).Value;
		}

		public void SetNotifyDuration(Duration? value)
		{
			if (value == null)
			{
				this.NotifyDurationStr = null;
				return;
			}

			this.NotifyDurationStr = DurationPattern.Roundtrip.Format((Duration)value);
		}

		public override string ToString()
		{
			return "Event: " + this.Name;
		}

		public Occurance? GetRepeatOccurance(IsoDayOfWeek day)
		{
			if (this.BaseOccurance == null)
				return null;

			Occurance? repeat = this.GetRepeat(day);

			if (repeat == null)
				return null;

			LocalDateTime nowLocal = TimeUtils.Now.InUtc().LocalDateTime;

			LocalDate nextDay;
			if (nowLocal.DayOfWeek == day)
			{
				nextDay = nowLocal.Date;

				if (repeat.GetInstant(nextDay) < TimeUtils.Now)
				{
					nextDay = nowLocal.Next(day).Date;
				}
			}
			else
			{
				nextDay = nowLocal.Next(day).Date;
			}

			return new Occurance(repeat, nextDay, this.BaseOccurance);
		}

		public void GetRepeatOccurance(IsoDayOfWeek day, ref List<Occurance> occurances)
		{
			Occurance? occurance = this.GetRepeatOccurance(day);

			if (occurance == null)
				return;

			Instant instant = occurance.GetInstant();
			if (instant < TimeUtils.Now)
				return;

			// don't get repeats that are more than 7 days away (because they are todays!)
			Duration durationTill = occurance.GetInstant() - TimeUtils.Now;
			if (durationTill.Days >= 7)
				return;

			// Does this occurrence happen to be the same as any other? skip.
			foreach (Occurance existingOccurence in occurances)
			{
				if (existingOccurence.GetInstant() == instant)
				{
					return;
				}
			}

			occurances.Add((Occurance)occurance);
		}

		public List<Occurance> GetNextOccurances()
		{
			List<Occurance> occurances = new List<Occurance>();

			// Is the original occurrence in the future? add it.
			if (this.BaseOccurance != null && this.BaseOccurance.GetInstant() > TimeUtils.Now)
				occurances.Add(this.BaseOccurance);

			this.GetRepeatOccurance(IsoDayOfWeek.Monday, ref occurances);
			this.GetRepeatOccurance(IsoDayOfWeek.Tuesday, ref occurances);
			this.GetRepeatOccurance(IsoDayOfWeek.Wednesday, ref occurances);
			this.GetRepeatOccurance(IsoDayOfWeek.Thursday, ref occurances);
			this.GetRepeatOccurance(IsoDayOfWeek.Friday, ref occurances);
			this.GetRepeatOccurance(IsoDayOfWeek.Saturday, ref occurances);
			this.GetRepeatOccurance(IsoDayOfWeek.Sunday, ref occurances);

			return occurances;
		}

		public Occurance? GetNextOccurance()
		{
			List<Occurance> occurances = this.GetNextOccurances();
			if (occurances == null || occurances.Count <= 0)
				return null;

			occurances.Sort((a, b) =>
			{
				return a.GetInstant().CompareTo(b.GetInstant());
			});

			return occurances[0];
		}

		public bool DoesRepeat()
		{
			bool repeat = false;

			for (int i = 1; i < 8; i++)
			{
				IsoDayOfWeek day = (IsoDayOfWeek)i;
				repeat |= this.DoesRepeat(day);
			}

			return repeat;
		}

		public bool DoesRepeat(IsoDayOfWeek day)
		{
			return this.GetRepeat(day) != null;
		}

		public Occurance? GetRepeat(IsoDayOfWeek day)
		{
			switch (day)
			{
				case IsoDayOfWeek.Monday: return this.Monday;
				case IsoDayOfWeek.Tuesday: return this.Tuesday;
				case IsoDayOfWeek.Wednesday: return this.Wednesday;
				case IsoDayOfWeek.Thursday: return this.Thursday;
				case IsoDayOfWeek.Friday: return this.Friday;
				case IsoDayOfWeek.Saturday: return this.Saturday;
				case IsoDayOfWeek.Sunday: return this.Sunday;
			}

			return null;
		}

		public Occurance SetRepeat(IsoDayOfWeek day, Occurance val)
		{
			switch (day)
			{
				case IsoDayOfWeek.Monday: return this.Monday = val;
				case IsoDayOfWeek.Tuesday: return this.Tuesday = val;
				case IsoDayOfWeek.Wednesday: return this.Wednesday = val;
				case IsoDayOfWeek.Thursday: return this.Thursday = val;
				case IsoDayOfWeek.Friday: return this.Friday = val;
				case IsoDayOfWeek.Saturday: return this.Saturday = val;
				case IsoDayOfWeek.Sunday: return this.Sunday = val;
			}

			return val;
		}

		[Serializable]
		public class Notification
		{
			public string? MessageId { get; set; }

			public List<Attendee> Attendees { get; set; } = new List<Attendee>();

			public class Attendee
			{
				public string? UserId { get; set; }

				public int? Status { get; set; }

				public string? RemindTime { get; set; }
			}
		}
	}
}
