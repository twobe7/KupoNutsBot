// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using NodaTime;

	public class Occurrence
	{
		public Instant Instant;
		public Duration Duration;

		public Occurrence(Instant instant, Duration duration)
		{
			this.Instant = instant;
			this.Duration = duration;
		}
	}
}
