// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class User : EntryBase
	{
		public ulong DiscordUserId { get; set; }
		public ulong DiscordGuildId { get; set; }
		public bool Banned { get; set; } = false;
		public List<Warning> Warnings { get; set; } = new List<Warning>();
		public List<Character> Characters { get; set; } = new List<Character>();

		[Obsolete]
		public uint FFXIVCharacterId { get; set; } = 0;

		[Serializable]
		public class Warning
		{
			public enum Actions
			{
				Unknown,
				PostRemoved,
				Warned,
			}

			public Actions Action { get; set; } = Actions.Unknown;
			public ulong ChannelId { get; set; } = 0;
			public string Comment { get; set; } = string.Empty;
		}

		[Serializable]
		public class Character
		{
			public uint FFXIVCharacterId { get; set; } = 0;
			public string? CharacterName { get; set; }
			public string? ServerName { get; set; }
			public string? FFXIVCharacterVerification { get; set; }
			public bool IsDefault { get; set; }
			public bool IsVerified { get; set; }
		}
	}
}
