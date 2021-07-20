// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Characters
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Discord;
	using XIVAPI;

	using FFXIVCollectCharacter = FFXIVCollect.CharacterAPI.Character;
	using XIVAPICharacter = XIVAPI.Character;

	public class CharacterInfo
	{
		public readonly uint Id;

		private FFXIVCollectCharacter? ffxivCollectCharacter;
		private XIVAPICharacter? xivApiCharacter;
		private FreeCompany? freeCompany;

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
		public XIVAPI.GrandCompany? GrandCompany => this.xivApiCharacter?.GrandCompany;
		public XIVAPI.FreeCompany? FreeCompany => this.freeCompany;
		public string? Server => this.xivApiCharacter?.Server;
		public string? DataCenter => this.xivApiCharacter?.DC;
		public string? Bio => this.xivApiCharacter?.Bio;

		public bool HasMounts => this.ffxivCollectCharacter != null && this.ffxivCollectCharacter.Mounts != null;
		public bool HasMinions => this.ffxivCollectCharacter != null && this.ffxivCollectCharacter.Mounts != null;
		public bool HasAchievements => this.ffxivCollectCharacter != null && this.ffxivCollectCharacter.Mounts != null;

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

			return this.xivApiCharacter.GetGear();
		}

		public Embed GetAttributesEmbed()
		{
			if (this.xivApiCharacter == null)
				throw new Exception("No XIVAPI character");

			return this.xivApiCharacter.GetAttributtes();
		}

		public async Task<Embed> GetElementalLevelEmbed()
		{
			XIVAPI.CharacterAPI.GetResponse charResponse = await XIVAPI.CharacterAPI.Get(this.Id, columns: "Character.ClassJobsElemental");

			if (charResponse.Character == null)
				throw new Exception("No character found.");

			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = this.xivApiCharacter.Name;
			builder.Description = "Elemental Level: " + (charResponse.Character?.GetElementalLevel() ?? 0);

			return builder.Build();
		}

		public async Task<Embed> GetResistanceRankEmbed()
		{
			XIVAPI.CharacterAPI.GetResponse charResponse = await XIVAPI.CharacterAPI.Get(this.Id, columns: "Character.ClassJobsBozjan");

			if (charResponse.Character == null)
				throw new Exception("No character found.");

			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = this.xivApiCharacter.Name;
			builder.Description = "Resistance Rank: " + (charResponse.Character?.GetResistanceRank() ?? 0) + "\n" + "Current Mettle: " + (charResponse.Character?.GetResistanceMettle() ?? 0);

			return builder.Build();
		}

		private async Task UpdateXivApi()
		{
			CharacterAPI.GetResponse charResponse = await CharacterAPI.Get(this.Id, CharacterAPI.CharacterData.ClassJobs | CharacterAPI.CharacterData.FreeCompany);

			if (charResponse.Character == null)
				throw new UserException("I couldn't find that character.");

			this.xivApiCharacter = charResponse.Character;
			this.freeCompany = charResponse.FreeCompany;
		}

		private async Task UpdateFfxivCollect()
		{
			this.ffxivCollectCharacter = await FFXIVCollect.CharacterAPI.Get(this.Id);
		}
	}
}
