// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;
	using System.Collections.Generic;
	using FC.Attributes;

	public class GuildSettings : SettingsEntry
	{
		public GuildSettings()
		{
			this.TimeZone = new List<string>();
		}

		public string Prefix { get; set; } = "?";
		public string NickName { get; set; } = "Kupo Nuts";

		[InspectorChannel]
		public string? LogChannel { get; set; }

		[InspectorChannel]
		public string? LodestoneChannel { get; set; }

		[InspectorChannel]
		public string? FashionReportChannel { get; set; }

		[InspectorTimeZone]
		public List<string> TimeZone { get; set; }
	}
}
