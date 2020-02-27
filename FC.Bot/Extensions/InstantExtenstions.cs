// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Extensions
{
	using System;
	using System.Collections.Generic;
	using NodaTime;

	public static class InstantExtenstions
	{
		public static bool IsApproximately(this Instant self, Instant other, long secondsAccuracy = 60)
		{
			if (self == other)
				return true;

			long selfSeconds = self.ToUnixTimeSeconds();
			long otherSeconds = other.ToUnixTimeSeconds();

			if (Math.Abs(otherSeconds - selfSeconds) < secondsAccuracy)
				return true;

			return false;
		}
	}
}
