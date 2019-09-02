// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public static class CharacterAPI
	{
		[Flags]
		public enum CharacterData
		{
			None = 0,
			Achievements = 1,
			FriendsList = 2,
			FreeCompany = 4,
			FreeCompanyMembers = 8,
			PlayerVsPlayerTeam = 16,
		}

		/// <summary>
		/// Get Character data, this is parsed straight from Lodestone in real-time. The more data you request the slower the entire request will be.
		/// </summary>
		public static async Task<Response> Get(uint? id, CharacterData dataFlags = CharacterData.None)
		{
			string data = string.Empty;

			if (FlagsUtils.IsSet(dataFlags, CharacterData.Achievements))
				data += "AC,";

			if (FlagsUtils.IsSet(dataFlags, CharacterData.FriendsList))
				data += "FR,";

			if (FlagsUtils.IsSet(dataFlags, CharacterData.FreeCompany))
				data += "FC,";

			if (FlagsUtils.IsSet(dataFlags, CharacterData.FreeCompanyMembers))
				data += "FCM,";

			if (FlagsUtils.IsSet(dataFlags, CharacterData.PlayerVsPlayerTeam))
				data += "PVP,";

			return await Request.Send<Response>("/character/" + id + "?data=" + data);
		}

		[Serializable]
		public class Response
		{
			public Achievements? Achievements;
			public bool? AchievementsPublic;
			public Character? Character;
			public FreeCompany? FreeCompany;
			public List<Member>? FreeCompanyMembers;
			////public List<>? Friends;
			public bool? FriendsPublic;
			public uint? PvPTeam;
		}

		[Serializable]
		public class FreeCompany
		{
			public string? Active;
			public uint? ActiveMemberCount;
			public List<string>? Crest;
			public string? DC;
			public Estate? Estate;
			public List<IconNameStatus>? Focus;
			public uint? Formed;
			public string? GrandCompany;
			public string? ID;
			public string? Name;
			public uint? ParseDate;
			public uint? Rank;
			public Ranking? Ranking;
			public string? Recruitment;
			public List<Reputation>? Reputation;
			public List<IconNameStatus>? Seeking;
			public string? Server;
			public string? Slogan;
			public string? Tag;

			public override string? ToString()
			{
				return "Free Company: " + this.Name + " (" + this.ID + ")";
			}
		}

		[Serializable]
		public class Member
		{
			public string? Avatar;
			public uint? FeastMatches;
			public uint? ID;
			public string? Name;
			public string? Rank;
			public string? RankIcon;
			public string? Server;

			public override string? ToString()
			{
				return "Member: " + this.Name + " (" + this.ID + ")";
			}
		}

		[Serializable]
		public class Reputation
		{
			public string? Name;
			public uint? Progress;
			public string? Rank;

			public override string? ToString()
			{
				return this.Name + " " + this.Progress + " " + this.Rank;
			}
		}

		[Serializable]
		public class Ranking
		{
			public uint? Monthly;
			public uint? Weekly;
		}

		[Serializable]
		public class Estate
		{
			public string? Greeting;
			public string? Name;
			public string? Plot;
		}

		[Serializable]
		public class IconNameStatus
		{
			public string? Icon;
			public string? Name;
			public bool Status;
		}

		[Serializable]
		public class Character
		{
			public Job? ActiveClassJob;
			public string? Avatar;
			public string? Bio;
			public List<Job>? ClassJobs;
			public string? DC;
			public string? FreeCompanyId;
			public GearSet? GearSet;
			public uint? Gender;
			public GrandCompany? GrandCompany;
			public uint? GuardianDeity;
			public uint? ID;
			////public List<>? Minions;
			////public List<>? Mounts;
			public string? Name;
			public string? Nameday;
			public uint? ParseDate;
			public string? Portrait;
			public uint? PvPTeamId;
			public uint? Race;
			public string? Server;
			public uint? Title;
			public bool TitleTop;
			public uint? Town;
			public uint? Tribe;

			public override string? ToString()
			{
				return "Character: " + this.Name + " (" + this.ID + ")";
			}
		}

		[Serializable]
		public class Job
		{
			public uint? ClassID;
			public ulong ExpLevel;
			public ulong ExpLevelMax;
			public ulong ExpLevelToGo;
			public bool IsSpecialised;
			public uint? JobID;
			public uint? Level;
			public string? Name;

			public override string? ToString()
			{
				return "Job: " + this.Name + " (" + this.JobID + ")";
			}
		}

		[Serializable]
		public class GearSet
		{
			public Dictionary<string, uint>? Attributes;
			public uint? ClassID;
			public Dictionary<GearSlots, Item>? Gear;
			public string? GearKey;
			public uint? JobID;
			public uint? Level;

			public enum GearSlots
			{
				Body,
				Bracelets,
				Earrings,
				Feet,
				Hands,
				Head,
				Legs,
				MainHand,
				Necklace,
				Ring1,
				Ring2,
				SoulCrystal,
				Waist,
			}
		}

		[Serializable]
		public class Item
		{
			public string? Creator;
			public uint? Dye;
			public uint? ID;
			public List<uint>? Materia;
			public uint? Mirage;
		}

		[Serializable]
		public class GrandCompany
		{
			public uint? NameID;
			public uint? RankID;
		}

		[Serializable]
		public class Achievements
		{
			public List<Achievement>? List;
			public uint? Pouints;
		}

		[Serializable]
		public class Achievement
		{
			public uint? Date;
			public uint? ID;
		}
	}
}
