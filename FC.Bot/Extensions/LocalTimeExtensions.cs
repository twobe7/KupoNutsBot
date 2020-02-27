// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Extensions
{
	using System.Globalization;
	using NodaTime;

	public static class LocalTimeExtensions
	{
		public static string ToDisplayString(this LocalTime self)
		{
			return self.ToString(@"HH:mm", CultureInfo.InvariantCulture);
		}
	}
}
