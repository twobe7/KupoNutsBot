// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;

	[Serializable]
	public abstract class SettingsEntry : EntryBase
	{
		public string GuildId { get; set; } = string.Empty;
	}
}
