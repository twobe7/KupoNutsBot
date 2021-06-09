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
		public int TotalKupoNutsReceived { get; set; } = 10;
		public int TotalKupoNutsCurrent { get; set; } = 10;
		public DateTime? LastDailyNut { get; set; }
		public int Reputation { get; set; } = 1;
		public DateTime? LastRepGiven { get; set; }
		public int TotalXPCurrent { get; set; } = 50;
		public int Level
		{
			get
			{
				if (this.TotalXPCurrent < 83)
					return 1;

				if (this.TotalXPCurrent < 174)
					return 2;

				int level = 2;
				double xpForLevel = 83;

				while (this.TotalXPCurrent >= Math.Floor(xpForLevel))
				{
					xpForLevel += 0.25 * Math.Floor(level + (300 * Math.Pow(2, level / 7d)));
					level++;
				}

				return level;
			}
		}

		public Dictionary<string, int> Inventory { get; set; } = new Dictionary<string, int>();

		[Obsolete]
		public uint FFXIVCharacterId { get; set; } = 0;

		public async void UpdateTotalKupoNuts(int kupoNuts, bool dailyNut = false)
		{
			this.TotalKupoNutsCurrent += kupoNuts;
			if (kupoNuts > 0)
			{
				this.TotalKupoNutsReceived += kupoNuts;
				if (dailyNut)
					this.LastDailyNut = DateTime.Now;
			}

			await UserService.SaveUser(this);
		}

		public void UpdateTotalKupoNuts(uint kupoNuts, bool dailyNut = false)
		{
			try
			{
				int nuts = (int)kupoNuts;
				this.UpdateTotalKupoNuts(nuts);
			}
			catch (Exception)
			{
				// Much nuts, wow.
			}
		}

		public async void ClearTotalKupoNuts()
		{
			this.TotalKupoNutsCurrent = 0;
			this.TotalKupoNutsReceived = 0;

			await UserService.SaveUser(this);
		}

		public async void UpdateInventory(string itemName, int quantity)
		{
			if (this.Inventory.ContainsKey(itemName))
			{
				this.Inventory[itemName] += quantity;

				// Remove if zero or below
				if (this.Inventory[itemName] <= 0)
					this.Inventory.Remove(itemName);
			}
			else
			{
				this.Inventory.Add(itemName, quantity);
			}

			await UserService.SaveUser(this);
		}

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
