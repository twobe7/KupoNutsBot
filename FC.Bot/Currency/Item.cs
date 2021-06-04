// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Currency
{
	using Discord;

	public class Item : EntryBase
	{
		public Item(string name, string description, int cost, IEmote reaction)
		{
			this.Name = name;
			this.Description = description;
			this.Cost = cost;
			this.Reaction = reaction;
		}

		public string Name { get; set; }

		public string Description { get; set; }

		public int Cost { get; set; }

		public IEmote Reaction { get; set; }

		public string? Macro { get; set; }
	}
}