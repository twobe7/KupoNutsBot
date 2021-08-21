// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Shop
{
	using System;
	using System.Collections.Generic;
	using Discord;
	using FC.Attributes;
	using FC.Utils;
	using NodaTime;

	[Serializable]
	public class ShopItem : EntryBase
	{
		public ShopItem()
		{
		}

		public ShopItem(string name, string desc, int cost, string reaction)
		{
			this.Name = name;
			this.Description = desc;
			this.Cost = cost;
			this.Reaction = reaction;
		}

		public ulong GuildId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public int Cost { get; set; } = 0;

		public string Reaction { get; set; } = @"<:kupo_nut:815575569482776607>";

		public string Role { get; set; } = string.Empty;
		public string RoleName { get; set; } = string.Empty;

		public IEmote ReactionEmote
		{
			get
			{
				try
				{
					return Emote.Parse(this.Reaction);
				}
				catch
				{
					return new Emoji(this.Reaction);
				}
			}
		}

		////public string? Macro { get; set; }
	}
}
