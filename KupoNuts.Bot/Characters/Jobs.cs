// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;

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
		Whitemage = 24,
		Scholar = 28,
		Astrologian = 33,
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

	#pragma warning disable SA1649
	public static class JobsExtensions
	{
		public static string AlchemistEmote = "<:alchemist:624832161094172722>";
		public static string ArmorerEmote = "<:armorer:624832160762953750>";
		public static string AstrologianEmote = "<:astrologian:624832162876882954>";
		public static string BardEmote = "<:bard:624832161861599232>";
		public static string BlackmageEmote = "<:blackmage:624832163862282270>";
		public static string BlacksmithEmote = "<:blacksmith:624832161610203155>";
		public static string BluemageEmote = "<:bluemage:624832162713042945>";
		public static string BotanistEmote = "<:botanist:624832162264383488>";
		public static string CarpenterEmote = "<:carpenter:624832162885009444>";
		public static string CulinarianEmote = "<:culinarian:624832161836433428>";
		public static string DancerEmote = "<:xivdancer:624832161932902411>";
		public static string DarkknightEmote = "<:darkknight:624832163166289940>";
		public static string DragoonEmote = "<:dragoon:624832161882570752>";
		public static string FisherEmote = "<:fisher:624832162214051860>";
		public static string GoldsmithEmote = "<:goldsmith:624832163791110174>";
		public static string GunbreakerEmote = "<:gunbreaker:624832162398601244>";
		public static string LeatherworkerEmote = "<:leatherworker:624832162461384704>";
		public static string MachinistEmote = "<:machinist:624832161996079144>";
		public static string MinerEmote = "<:miner:624832162180497440>";
		public static string MonkEmote = "<:monk:624832163262627860>";
		public static string NinjaEmote = "<:ninja:624832163342188544>";
		public static string PaladinEmote = "<:paladin:624832161677312016>";
		public static string RedmageEmote = "<:redmage:624832162046410762>";
		public static string SamuraiEmote = "<:samurai:624832162184560653>";
		public static string ScholarEmote = "<:scholar:624832162197274654>";
		public static string SummonerEmote = "<:summoner:624832162230960128>";
		public static string WarriorEmote = "<:warrior:624832162079703050>";
		public static string WeaverEmote = "<:weaver:624832162247475200>";
		public static string WhitemageEmote = "<:whitemage:624832162998255637>";

		public static string GetEmote(this Jobs self)
		{
			switch (self)
			{
				case Jobs.Paladin: return PaladinEmote;
				case Jobs.Warrior: return WarriorEmote;
				case Jobs.Darkknight: return DarkknightEmote;
				case Jobs.Gunbreaker: return GunbreakerEmote;
				case Jobs.Monk: return MonkEmote;
				case Jobs.Dragoon: return DragoonEmote;
				case Jobs.Ninja: return NinjaEmote;
				case Jobs.Samurai: return SamuraiEmote;
				case Jobs.Whitemage: return WhitemageEmote;
				case Jobs.Scholar: return ScholarEmote;
				case Jobs.Astrologian: return AstrologianEmote;
				case Jobs.Bard: return BardEmote;
				case Jobs.Machinist: return MachinistEmote;
				case Jobs.Dancer: return DancerEmote;
				case Jobs.Blackmage: return BlackmageEmote;
				case Jobs.Summoner: return SummonerEmote;
				case Jobs.Redmage: return RedmageEmote;
				case Jobs.Bluemage: return BluemageEmote;
				case Jobs.Carpenter: return CarpenterEmote;
				case Jobs.Blacksmith: return BlacksmithEmote;
				case Jobs.Armorer: return ArmorerEmote;
				case Jobs.Goldsmith: return GoldsmithEmote;
				case Jobs.Leatherworker: return LeatherworkerEmote;
				case Jobs.Weaver: return WeaverEmote;
				case Jobs.Alchemist: return AlchemistEmote;
				case Jobs.Culinarian: return CulinarianEmote;
				case Jobs.Miner: return MinerEmote;
				case Jobs.Botanist: return BotanistEmote;
				case Jobs.Fisher: return FisherEmote;
			}

			throw new Exception("unknoiwn job:\"" + self + "\"");
		}
	}
}
