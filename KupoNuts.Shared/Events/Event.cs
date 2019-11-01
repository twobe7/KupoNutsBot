// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Text.Json.Serialization;
	using Amazon.DynamoDBv2.DataModel;
	using KupoNuts.Utils;
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

		#pragma warning disable SA1516
		public string ServerIdStr { get; set; } = "391492798353768449";
		public string? ChannelId { get; set; }
		public string Name { get; set; } = "New Event";
		public string? ShortDescription { get; set; }
		public string? Description { get; set; }
		public string? Message { get; set; }
		public string? Image { get; set; }
		public Colors Color { get; set; }

		public Occurance BaseOccurance { get; set; } = new Occurance();
		public Occurance? Monday { get; set; }
		public Occurance? Tuesday { get; set; }
		public Occurance? Wednesday { get; set; }
		public Occurance? Thursday { get; set; }
		public Occurance? Friday { get; set; }
		public Occurance? Saturday { get; set; }
		public Occurance? Sunday { get; set; }

		public string? RemindMeEmote { get; set; }
		public string? NotifyDurationStr { get; set; }
		public List<Status> Statuses { get; set; } = new List<Status>();
		public Notification? Notify { get; set; }

		[DynamoDBIgnore]
		[JsonIgnore]
		public ulong ServerId
		{
			get
			{
				return ulong.Parse(this.ServerIdStr);
			}
			set
			{
				this.ServerIdStr = value.ToString();
			}
		}

		[DynamoDBIgnore]
		[JsonIgnore]
		public Duration? NotifyDuration
		{
			get
			{
				if (string.IsNullOrEmpty(this.NotifyDurationStr))
					return null;

				return DurationPattern.Roundtrip.Parse(this.NotifyDurationStr).Value;
			}

			set
			{
				if (value == null)
				{
					this.NotifyDurationStr = null;
					return;
				}

				this.NotifyDurationStr = DurationPattern.Roundtrip.Format((Duration)value);
			}
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

			// dont get repeats that are more than 7 days away (because they are todays!)
			Duration durationTill = occurance.GetInstant() - TimeUtils.Now;
			if (durationTill.Days >= 7)
				return;

			// Does this occurance happen to be the same as any other? skip.
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

			// Is the original occurance in the future? add it.
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
		public class Occurance
		{
			public Occurance()
			{
			}

			public Occurance(LocalDate date, LocalTime time)
			{
				this.Time = time;
				this.Date = date;
			}

			public Occurance(Occurance repeat, LocalDate date, Occurance baseOccurance)
			{
				this.Time = repeat.Time ?? baseOccurance.Time;
				this.Duration = repeat.Duration;
				this.Date = date;
			}

			public string? TimeStr { get; set; }
			public string? DurationStr { get; set; }
			public string? DateStr { get; set; }

			[DynamoDBIgnore]
			[JsonIgnore]
			public Duration Duration
			{
				get
				{
					if (string.IsNullOrEmpty(this.DurationStr))
						return NodaTime.Duration.FromSeconds(0);

					return DurationPattern.Roundtrip.Parse(this.DurationStr).Value;
				}
				set
				{
					this.DurationStr = DurationPattern.Roundtrip.Format(value);
				}
			}

			[DynamoDBIgnore]
			[JsonIgnore]
			public LocalTime? Time
			{
				get
				{
					if (string.IsNullOrEmpty(this.TimeStr))
						return null;

					return LocalTimePattern.ExtendedIso.Parse(this.TimeStr).Value;
				}

				set
				{
					if (value == null)
					{
						this.TimeStr = null;
						return;
					}

					this.TimeStr = LocalTimePattern.ExtendedIso.Format((LocalTime)value);
				}
			}

			[DynamoDBIgnore]
			[JsonIgnore]
			public LocalDate? Date
			{
				get
				{
					if (string.IsNullOrEmpty(this.DateStr))
						return null;

					return LocalDatePattern.Iso.Parse(this.DateStr).Value;
				}

				set
				{
					if (value == null)
					{
						this.DateStr = null;
						return;
					}

					this.DateStr = LocalDatePattern.Iso.Format((LocalDate)value);
				}
			}

			public Instant GetInstant(LocalDate? defaultDate = null, LocalTime? defaultTime = null)
			{
				LocalDate? date = this.Date ?? defaultDate;
				LocalTime? time = this.Time ?? defaultTime;

				if (date == null)
					throw new Exception("No Date in event occurance");

				if (time == null)
					throw new Exception("No Time in event occurance");

				LocalDateTime ldt = (LocalDate)date + (LocalTime)time;
				return ldt.InUtc().ToInstant();
			}
		}

		[Serializable]
		public class Status
		{
			public Status()
			{
			}

			public Status(string emote, string? display = null)
			{
				this.EmoteString = emote;
				this.Display = display;
			}

			public string? EmoteString { get; set; }

			public string? Display { get; set; }
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
