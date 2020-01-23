// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Events
{
	using System;
	using NodaTime;
	using NodaTime.Text;

	[Serializable]
	public class Occurance
	{
		public Occurance()
		{
		}

		public Occurance(LocalDate date, LocalTime time)
		{
			this.SetLocalTime(time);
			this.SetLocalDate(date);
		}

		public Occurance(Occurance repeat, LocalDate date, Occurance baseOccurance)
		{
			LocalTime? time = this.GetLocalTime() ?? baseOccurance.GetLocalTime();

			this.SetLocalTime(time);
			this.SetDuration(repeat.GetDuration());
			this.SetLocalDate(date);
		}

		public string? TimeStr { get; set; }
		public string? DurationStr { get; set; }
		public string? DateStr { get; set; }

		public LocalDate? GetLocalDate()
		{
			if (string.IsNullOrEmpty(this.DateStr))
				return null;

			return LocalDatePattern.Iso.Parse(this.DateStr).Value;
		}

		public void SetLocalDate(LocalDate? date)
		{
			if (date == null)
			{
				this.DateStr = null;
				return;
			}

			this.DateStr = LocalDatePattern.Iso.Format((LocalDate)date);
		}

		public LocalTime? GetLocalTime()
		{
			if (string.IsNullOrEmpty(this.TimeStr))
				return null;

			return LocalTimePattern.ExtendedIso.Parse(this.TimeStr).Value;
		}

		public void SetLocalTime(LocalTime? time)
		{
			if (time == null)
			{
				this.TimeStr = null;
				return;
			}

			this.TimeStr = LocalTimePattern.ExtendedIso.Format((LocalTime)time);
		}

		public Duration GetDuration()
		{
			if (string.IsNullOrEmpty(this.DurationStr))
				return NodaTime.Duration.FromSeconds(0);

			return DurationPattern.Roundtrip.Parse(this.DurationStr).Value;
		}

		public void SetDuration(Duration duration)
		{
			this.DurationStr = DurationPattern.Roundtrip.Format(duration);
		}

		public Instant GetInstant(LocalDate? defaultDate = null, LocalTime? defaultTime = null)
		{
			LocalDate? date = string.IsNullOrEmpty(this.DateStr) ? defaultDate : LocalDatePattern.Iso.Parse(this.DateStr).Value;
			LocalTime? time = string.IsNullOrEmpty(this.TimeStr) ? defaultTime : LocalTimePattern.ExtendedIso.Parse(this.TimeStr).Value;

			if (date == null)
				throw new Exception("No Date in event occurrence");

			if (time == null)
				throw new Exception("No Time in event occurrence");

			LocalDateTime ldt = (LocalDate)date + (LocalTime)time;
			return ldt.InUtc().ToInstant();
		}
	}
}
