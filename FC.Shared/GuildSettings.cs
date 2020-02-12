// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using FC.Attributes;

	public class GuildSettings : SettingsEntry
	{
		public string Prefix { get; set; } = "?";
		public string NickName { get; set; } = string.Empty;

		[InspectorChannel]
		public string? LogChannel { get; set; }
	}
}
