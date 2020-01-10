// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Quotes
{
	using System;
	using NodaTime;
	using NodaTime.Text;

	public static class QuoteExtensions
	{
		public static void SetDateTime(this Quote self, Instant instant)
		{
			self.DateTime = InstantPattern.ExtendedIso.Format(instant);
		}

		public static void SetDateTime(this Quote self, DateTimeOffset dateTimeOffset)
		{
			self.SetDateTime(Instant.FromDateTimeOffset(dateTimeOffset));
		}

		public static Instant GetDateTime(this Quote self)
		{
			if (string.IsNullOrEmpty(self.DateTime))
				return Instant.FromJulianDate(0);

			return InstantPattern.ExtendedIso.Parse(self.DateTime).Value;
		}
	}
}
