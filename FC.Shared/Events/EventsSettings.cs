// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class EventsSettings : SettingsEntry
	{
		public string? CalendarChannel { get; set; }
		public string? CalendarMessage { get; set; }
		public string? CalendarMessage2 { get; set; }
	}
}
