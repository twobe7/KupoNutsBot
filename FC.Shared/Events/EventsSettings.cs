﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using FC.Attributes;

	[Serializable]
	public class EventsSettings : SettingsEntry
	{
		[InspectorChannel]
		public string? CalendarChannel { get; set; }

		[InspectorHidden]
		public string? CalendarWeekMessageId { get; set; }

		[InspectorHidden]
		public string? CalendarFutureMessageId { get; set; }
	}
}
