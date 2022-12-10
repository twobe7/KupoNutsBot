// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Characters
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Discord;
	using NetStone;
	using NetStone.Model.Parseables.Character;
	using XIVAPI;

	using FFXIVCollectCharacter = FFXIVCollect.CharacterAPI.Character;
	using XIVAPICharacter = XIVAPI.Character;

	public class CharacterInfo
	{
		public readonly uint Id;

		private FFXIVCollectCharacter? ffxivCollectCharacter;
		private XIVAPICharacter? xivApiCharacter;
		private FreeCompany? freeCompany;

		private NetStone.Model.Parseables.Character.Gear.CharacterGear? netStoneGear;

		public CharacterInfo(uint id)
		{
			this.Id = id;
		}

		public enum Races
		{
			None = 0,
			Hyur = 1,
			Elezen = 2,
			Lalafell = 3,
			[Description("Miqo'te")]
			Miqote = 4,
			Roegadyn = 5,
			[Description("Au Ra")]
			AuRa = 6,
			Hrothgar = 7,
			Viera = 8,
		}

		public enum Tribes
		{
			None = 0,
			Midlander = 1,
			Highlander = 2,
			Wildwood = 3,
			Duskwight = 4,
			Plainsfolk = 5,
			Dunesfolk = 6,
			[Description("Seekers Of The Sun")]
			SeekersOfTheSun = 7,
			[Description("Keepers Of The Moon")]
			KeepersOfTheMoon = 8,
			[Description("Sea Wolves")]
			SeaWolves = 9,
			Hellsguard = 10,
			Raen = 11,
			Xaela = 12,
			Helions = 13,
			[Description("The Lost")]
			TheLost = 14,
			Rava = 15,
			Veena = 16,
		}

#pragma warning disable IDE0025, SA1516, CS8602
		public string? Portrait => this.xivApiCharacter?.Portrait;
		public string? Name => this.xivApiCharacter?.Name;
		public string? Title => this.xivApiCharacter?.Title?.Name;
		public string? Race => this.xivApiCharacter?.Race?.Name;
		public string? Tribe => this.xivApiCharacter?.Tribe?.Name;
		public string? Town => this.xivApiCharacter?.Town?.Name;
		public string? NameDay => this.xivApiCharacter?.Nameday;
		public Data? GuardianDeity => this.xivApiCharacter?.GuardianDeity;
		public GrandCompany? GrandCompany => this.xivApiCharacter?.GrandCompany;
		public FreeCompany? FreeCompany => this.freeCompany;
		public string? Server => this.xivApiCharacter?.Server;
		public string? DataCenter => this.xivApiCharacter?.DC;
		public string? Bio => this.xivApiCharacter?.Bio;

		public bool HasMounts => this.ffxivCollectCharacter != null && this.ffxivCollectCharacter.Mounts != null;
		public bool HasMinions => this.ffxivCollectCharacter != null && this.ffxivCollectCharacter.Minions != null;
		public bool HasAchievements => this.ffxivCollectCharacter != null && this.ffxivCollectCharacter.Achievements != null;

		public (int Count, int Total) Mounts
		{
			get
			{
				if (!this.HasMounts)
					return (0, 0);

				return (this.ffxivCollectCharacter.Mounts.Count, this.ffxivCollectCharacter.Mounts.Total);
			}
		}

		public (int Count, int Total) Minions
		{
			get
			{
				if (!this.HasMinions)
					return (0, 0);

				return (this.ffxivCollectCharacter.Minions.Count, this.ffxivCollectCharacter.Minions.Total);
			}
		}

		public (int Count, int Total) Achievements
		{
			get
			{
				if (!this.HasAchievements)
					return (0, 0);

				return (this.ffxivCollectCharacter.Achievements.Count, this.ffxivCollectCharacter.Achievements.Total);
			}
		}

		public async Task Update()
		{
			Task xivApi = Task.Run(this.UpdateXivApi);
			Task ffxivCollect = Task.Run(this.UpdateFfxivCollect);

			await xivApi;
			await ffxivCollect;
		}

		public string GetJobLevel(Jobs job)
		{
			ClassJob? classJob = this.xivApiCharacter?.GetClassJob(job);

			if (classJob == null)
				return string.Empty;

			return (classJob.Level ?? 0).ToString();
		}

		public Embed GetGearEmbed()
		{
			if (this.xivApiCharacter == null)
				throw new Exception("No XIVAPI character");

			return this.xivApiCharacter.GetGear(this.GetNetStoneGear());
		}

		public Embed GetAttributesEmbed()
		{
			if (this.xivApiCharacter == null)
				throw new Exception("No XIVAPI character");

			return this.xivApiCharacter.GetAttributtes();
		}

		public async Task<Embed> GetElementalLevelEmbed()
		{
			////XIVAPI.CharacterAPI.GetResponse charResponse = await XIVAPI.CharacterAPI.Get(this.Id, columns: "Character.ClassJobsElemental");

			// Get Client
			LodestoneClient client = await LodestoneClient.GetClientAsync();
			LodestoneCharacter? character = await client.GetCharacter(this.Id.ToString());

			if (character == null)
				throw new Exception("No character found.");

			// Set level
			int level = 0;
			long exp = 0;

			// Get Elemental Level
			NetStone.Model.Parseables.Character.ClassJob.CharacterClassJob? classJobInfo = await character?.GetClassJobInfo();

			// Try get the Level
			try
			{
				level = classJobInfo.Eureka.Level;
				exp = classJobInfo.Eureka.ExpCurrent;
			}
			catch
			{
			}

			EmbedBuilder builder = new EmbedBuilder
			{
				Title = this.xivApiCharacter.Name,
				Description = $"Elemental Level: {level}\nExperience: {exp.ToString("N0")}",
			};

			return builder.Build();
		}

		public async Task<Embed> GetResistanceRankEmbed()
		{
			////XIVAPI.CharacterAPI.GetResponse charResponse = await XIVAPI.CharacterAPI.Get(this.Id, columns: "Character.ClassJobsBozjan");

			// Get Client
			LodestoneClient client = await LodestoneClient.GetClientAsync();
			LodestoneCharacter? character = await client.GetCharacter(this.Id.ToString());

			if (character == null)
				throw new Exception("No character found.");

			// Set level / mettle
			int level = 0;
			long mettle = 0;

			// Get Resistance Rank
			NetStone.Model.Parseables.Character.ClassJob.CharacterClassJob? classJobInfo = await character?.GetClassJobInfo();

			// Try get the Level and Mettle
			try
			{
				level = classJobInfo.Bozja.Level;
				mettle = classJobInfo.Bozja.ExpCurrent;
			}
			catch
			{
			}

			EmbedBuilder builder = new EmbedBuilder
			{
				Title = this.xivApiCharacter.Name,
				Description = $"Resistance Rank: {level}\nCurrent Mettle: {mettle.ToString("N0")}",
			};

			return builder.Build();
		}

		public NetStone.Model.Parseables.Character.Gear.CharacterGear? GetNetStoneGear()
		{
			return this.netStoneGear;
		}

		private async Task UpdateXivApi()
		{
			////CharacterAPI.GetResponse charResponse = await CharacterAPI.Get(this.Id, CharacterAPI.CharacterData.ClassJobs | CharacterAPI.CharacterData.FreeCompany);

			// Get Client
			LodestoneClient client = await LodestoneClient.GetClientAsync();
			LodestoneCharacter? character = await client.GetCharacter(this.Id.ToString());

			if (character == null)
				throw new UserException("I couldn't find that character.");

			this.xivApiCharacter = new XIVAPICharacter(character);

			this.netStoneGear = character.Gear;

			if (character.FreeCompany != null)
				this.freeCompany = new FreeCompany(await client.GetFreeCompany(character.FreeCompany.Id));
		}

		private async Task UpdateFfxivCollect()
		{
			this.ffxivCollectCharacter = await FFXIVCollect.CharacterAPI.Get(this.Id);
		}
	}
}
