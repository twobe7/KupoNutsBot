// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class Character
	{
		private static readonly Dictionary<string, string> ServerDataCentreLookup = new Dictionary<string, string>
		{
			// JP
			{ "Aegis", "Elemental" },
			{ "Atomos", "Elemental" },
			{ "Carbuncle", "Elemental" },
			{ "Garuda", "Elemental" },
			{ "Gungnir", "Elemental" },
			{ "Kujata", "Elemental" },
			{ "Ramuh", "Elemental" },
			{ "Tonberry", "Elemental" },
			{ "Typhon", "Elemental" },
			{ "Unicorn", "Elemental" },
			{ "Alexander", "Gaia" },
			{ "Bahamut", "Gaia" },
			{ "Durandal", "Gaia" },
			{ "Fenrir", "Gaia" },
			{ "Ifrit", "Gaia" },
			{ "Ridill", "Gaia" },
			{ "Tiamat", "Gaia" },
			{ "Ultima", "Gaia" },
			{ "Valefor", "Gaia" },
			{ "Yojimbo", "Gaia" },
			{ "Zeromus", "Gaia" },
			{ "Anima", "Mana" },
			{ "Asura", "Mana" },
			{ "Belias", "Mana" },
			{ "Chocobo", "Mana" },
			{ "Hades", "Mana" },
			{ "Ixion", "Mana" },
			{ "Mandragora", "Mana" },
			{ "Masamune", "Mana" },
			{ "Pandaemonium", "Mana" },
			{ "Shinryu", "Mana" },
			{ "Titan", "Mana" },

			// NA
			{ "Adamantoise", "Aether" },
			{ "Cactuar", "Aether" },
			{ "Faerie", "Aether" },
			{ "Gilgamesh", "Aether" },
			{ "Jenova", "Aether" },
			{ "Midgardsormr", "Aether" },
			{ "Sargatanas", "Aether" },
			{ "Siren", "Aether" },
			{ "Behemoth", "Primal" },
			{ "Excalibur", "Primal" },
			{ "Exodus", "Primal" },
			{ "Famfrit", "Primal" },
			{ "Hyperion", "Primal" },
			{ "Lamia", "Primal" },
			{ "Leviathan", "Primal" },
			{ "Ultros", "Primal" },
			{ "Balmung", "Crystal" },
			{ "Brynhildr", "Crystal" },
			{ "Coeurl", "Crystal" },
			{ "Diabolos", "Crystal" },
			{ "Goblin", "Crystal" },
			{ "Malboro", "Crystal" },
			{ "Mateus", "Crystal" },
			{ "Zalera", "Crystal" },

			// EU
			{ "Cerberus", "Chaos" },
			{ "Louisoix", "Chaos" },
			{ "Moogle", "Chaos" },
			{ "Omega", "Chaos" },
			{ "Ragnarok", "Chaos" },
			{ "Spriggan", "Chaos" },
			{ "Lich", "Light" },
			{ "Odin", "Light" },
			{ "Phoenix", "Light" },
			{ "Shiva", "Light" },
			{ "Twintania", "Light" },
			{ "Zodiark", "Light" },

			// OCE
			{ "Bismark", "Materia" },
			{ "Ravana", "Materia" },
			{ "Sephirot", "Materia" },
			{ "Sophia", "Materia" },
			{ "Zurvan", "Materia" },
		};

		private static readonly Dictionary<string, string> GuardianDietyIconLookup = new Dictionary<string, string>
		{
			{ "Halone, the Fury", "061601" },
			{ "Menphina, the Lover", "061602" },
			{ "Thaliak, the Scholar", "061603" },
			{ "Nymeia, the Spinner", "061604" },
			{ "Llymlaen, the Navigator", "061605" },
			{ "Oschon, the Wanderer", "061606" },
			{ "Byregot, the Builder", "061607" },
			{ "Rhalgr, the Destroyer", "061608" },
			{ "Azeyma, the Warden", "061609" },
			{ "Nald'thal, the Traders", "061610" },
			{ "Nophica, the Matron", "061611" },
			{ "Althyk, the Keeper", "061612" },
		};

		private static readonly Dictionary<string, int> GrandCompanyIconLookup = new Dictionary<string, int>
		{
			{ "Maelstrom", 1 },
			{ "Adder", 2 },
			{ "Flames", 3 },
		};

		private static readonly Dictionary<string, string> GrandCompanyRankIconLookup = new Dictionary<string, string>
		{
			// Maelstrom
			{ "Storm Private Third Class", "083001" },
			{ "Storm Private Second Class", "083002" },
			{ "Storm Private First Class", "083003" },
			{ "Storm Corporal", "083004" },
			{ "Storm Sergeant Third Class", "083005" },
			{ "Storm Sergeant Second Class", "083006" },
			{ "Storm Sergeant First Class", "083007" },
			{ "Chief Storm Sergeant", "083008" },
			{ "Second Storm Lieutenant", "083009" },
			{ "First Storm Lieutenant", "083010" },
			{ "Storm Captain", "083011" },
			{ "Second Storm Commander", "083012" },
			{ "First Storm Commander", "083013" },
			{ "High Storm Commander", "083014" },
			{ "Rear Storm Marshal", "083015" },
			{ "Vice Storm Marshal", "083016" },
			{ "Storm Marshal", "083017" },
			{ "Grand Storm Marshal", "083018" },
			{ "Storm Champion", "083019" },
			{ "Storm Leader", "083020" },

			// Twin Adder
			{ "Serpent Private Third Class", "083051" },
			{ "Serpent Private Second Class", "083052" },
			{ "Serpent Private First Class", "083053" },
			{ "Serpent Corporal", "083054" },
			{ "Serpent Sergeant Third Class", "083055" },
			{ "Serpent Sergeant Second Class", "083056" },
			{ "Serpent Sergeant First Class", "083057" },
			{ "Chief Serpent Sergeant", "083058" },
			{ "Second Serpent Lieutenant", "083059" },
			{ "First Serpent Lieutenant", "083060" },
			{ "Serpent Captain", "083061" },
			{ "Second Serpent Commander", "083062" },
			{ "First Serpent Commander", "083063" },
			{ "High Serpent Commander", "083064" },
			{ "Rear Serpent Marshal", "083065" },
			{ "Vice Serpent Marshal", "083066" },
			{ "Serpent Marshal", "083067" },
			{ "Grand Serpent Marshal", "083068" },
			{ "Serpent Champion", "083069" },
			{ "Serpent Leader", "083070" },

			// Immortal Flames
			{ "Flame Private Third Class", "083101" },
			{ "Flame Private Second Class", "083102" },
			{ "Flame Private First Class", "083103" },
			{ "Flame Corporal", "083104" },
			{ "Flame Sergeant Third Class", "083105" },
			{ "Flame Sergeant Second Class", "083106" },
			{ "Flame Sergeant First Class", "083107" },
			{ "Chief Flame Sergeant", "083108" },
			{ "Second Flame Lieutenant", "083109" },
			{ "First Flame Lieutenant", "083110" },
			{ "Flame Captain", "083111" },
			{ "Second Flame Commander", "083112" },
			{ "First Flame Commander", "083113" },
			{ "High Flame Commander", "083114" },
			{ "Rear Flame Marshal", "083115" },
			{ "Vice Flame Marshal", "083116" },
			{ "Flame Marshal", "083117" },
			{ "Grand Flame Marshal", "083118" },
			{ "Flame Champion", "083119" },
			{ "Flame Leader", "083120" },
		};

		private static readonly Dictionary<string, uint> GenderLookup = new Dictionary<string, uint>()
		{
			{ "♂", 1 },
			{ "♀", 2 },
		};

		public Character(NetStone.Model.Parseables.Character.LodestoneCharacter netChar)
		{
			this.Avatar = netChar.Avatar.ToString();
			this.Bio = netChar.Bio;
			this.Server = netChar.Server;
			this.DC = ServerDataCentreLookup.GetValueOrDefault(this.Server) ?? "Unknown";

			if (netChar.FreeCompany != null)
				this.FreeCompanyId = netChar.FreeCompany.Id;

			this.Race = new Data(name: netChar.Race);
			this.Tribe = new Data(name: netChar.Clan);
			this.Gender = GenderLookup.GetValueOrDefault(netChar.Gender);

			if (netChar.GrandCompanyName != null && netChar.GrandCompanyRank != null)
			{
				this.GrandCompany = new GrandCompany(
					GrandCompanyIconLookup.GetValueOrDefault(netChar.GrandCompanyName),
					netChar.GrandCompanyName,
					$"{GrandCompanyRankIconLookup.GetValueOrDefault(netChar.GrandCompanyRank)}.png",
					netChar.GrandCompanyRank);
			}

			this.Name = netChar.Name;
			this.Nameday = netChar.Nameday;
			this.Portrait = netChar.Portrait.ToString();
			this.Title = new Data(name: netChar.Title);

			this.Town = new Data(name: netChar.TownName);

			this.GuardianDeity = new Data(name: netChar.GuardianDeityName, icon: $"/i/061000/{GuardianDietyIconLookup.GetValueOrDefault(netChar.GuardianDeityName)}.png");

			var classJobInfo = netChar.GetClassJobInfo().Result;

			this.ClassJobs = new List<ClassJob>()
			{
				new ClassJob(Jobs.Alchemist, classJobInfo.Alchemist),
				new ClassJob(Jobs.Armorer, classJobInfo.Armorer),
				new ClassJob(Jobs.Astrologian, classJobInfo.Astrologian),
				new ClassJob(Jobs.Bard, classJobInfo.Bard),
				new ClassJob(Jobs.Blackmage, classJobInfo.BlackMage),
				new ClassJob(Jobs.Blacksmith, classJobInfo.Blacksmith),
				new ClassJob(Jobs.Bluemage, classJobInfo.BlueMage),
				new ClassJob(Jobs.Botanist, classJobInfo.Botanist),
				new ClassJob(Jobs.Carpenter, classJobInfo.Carpenter),
				new ClassJob(Jobs.Culinarian, classJobInfo.Culinarian),
				new ClassJob(Jobs.Dancer, classJobInfo.Dancer),
				new ClassJob(Jobs.Darkknight, classJobInfo.DarkKnight),
				new ClassJob(Jobs.Dragoon, classJobInfo.Dragoon),
				new ClassJob(Jobs.Fisher, classJobInfo.Fisher),
				new ClassJob(Jobs.Goldsmith, classJobInfo.Goldsmith),
				new ClassJob(Jobs.Gunbreaker, classJobInfo.Gunbreaker),
				new ClassJob(Jobs.Leatherworker, classJobInfo.Leatherworker),
				new ClassJob(Jobs.Machinist, classJobInfo.Machinist),
				new ClassJob(Jobs.Miner, classJobInfo.Miner),
				new ClassJob(Jobs.Monk, classJobInfo.Monk),
				new ClassJob(Jobs.Ninja, classJobInfo.Ninja),
				new ClassJob(Jobs.Paladin, classJobInfo.Paladin),
				new ClassJob(Jobs.Reaper, classJobInfo.Reaper),
				new ClassJob(Jobs.Redmage, classJobInfo.RedMage),
				new ClassJob(Jobs.Sage, classJobInfo.Sage),
				new ClassJob(Jobs.Samurai, classJobInfo.Samurai),
				new ClassJob(Jobs.Scholar, classJobInfo.Scholar),
				new ClassJob(Jobs.Summoner, classJobInfo.Summoner),
				new ClassJob(Jobs.Warrior, classJobInfo.Warrior),
				new ClassJob(Jobs.Weaver, classJobInfo.Weaver),
				new ClassJob(Jobs.Whitemage, classJobInfo.WhiteMage),
			};
		}

		// TODO: This enum is replicated from FC.Bot.Characters
		// It currently only exists here due to Netstone -> XIVAPI model mapping
		public enum Jobs
		{
			Paladin = 19,
			Warrior = 21,
			Darkknight = 32,
			Gunbreaker = 37,
			Monk = 20,
			Dragoon = 22,
			Ninja = 30,
			Samurai = 34,
			Reaper = 40,
			Whitemage = 24,
			Scholar = 28,
			Astrologian = 33,
			Sage = 39,
			Bard = 23,
			Machinist = 31,
			Dancer = 38,
			Blackmage = 25,
			Summoner = 27,
			Redmage = 35,
			Bluemage = 36,
			Carpenter = 8,
			Blacksmith = 9,
			Armorer = 10,
			Goldsmith = 11,
			Leatherworker = 12,
			Weaver = 13,
			Alchemist = 14,
			Culinarian = 15,
			Miner = 16,
			Botanist = 17,
			Fisher = 18,
		}

		public ClassJob? ActiveClassJob { get; set; }
		public string Avatar { get; set; } = string.Empty;
		public string Bio { get; set; } = string.Empty;
		public List<ClassJob>? ClassJobs { get; set; }
		public ClassJob? ClassJobsElemental { get; set; }
		public ClassJob? ClassJobsBozjan { get; set; }
		public string DC { get; set; } = string.Empty;
		public string FreeCompanyId { get; set; } = string.Empty;
		public GearSet? GearSet { get; set; }
		public uint Gender { get; set; }
		public GrandCompany? GrandCompany { get; set; }
		public Data? GuardianDeity { get; set; }
		public uint ID { get; set; }
		////public List<>? Minions;
		////public List<>? Mounts;
		public string Name { get; set; } = string.Empty;
		public string Nameday { get; set; } = string.Empty;
		public uint ParseDate { get; set; }
		public string Portrait { get; set; } = string.Empty;
		////public uint? PvPTeamId { get; set; }
		public Data? Race { get; set; }
		public string Server { get; set; } = string.Empty;
		public Data? Title { get; set; }
		public bool TitleTop { get; set; }
		public Data? Town { get; set; }
		public Data? Tribe { get; set; }

		public override string? ToString()
		{
			return "Character: " + this.Name + " (" + this.ID + ")";
		}
	}
}
