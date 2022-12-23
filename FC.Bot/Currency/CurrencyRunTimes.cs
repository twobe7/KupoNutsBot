// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Currency
{
	using System;
	using System.Collections.Generic;

	public class CurrencyRunTimes
	{
		public readonly Dictionary<ulong, DateTime?> SlotsLastRunTime = new Dictionary<ulong, DateTime?>();
		public Dictionary<ulong, DateTime?> ActiveInventoryWindows = new Dictionary<ulong, DateTime?>();
		public Dictionary<ulong, DateTime?> BlackjackLastRunTime = new Dictionary<ulong, DateTime?>();
		public Dictionary<ulong, uint> UserDailyGameCount = new Dictionary<ulong, uint>();
	}
}
