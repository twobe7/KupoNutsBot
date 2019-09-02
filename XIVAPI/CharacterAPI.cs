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
		/// Search and retrieve character data from The Lodestone. Providing useful information such as character profile data,
		/// minions and mounts obtained, achievements obtained and their relative dates. Character friends, their free company, pvp team and much more.
		/// </summary>
		/// <param name="name">The name to search for, you can use `+` for spaces or let the API handle it for you.</param>
		/// <param name="server">The server to search against, this is case sensitive - You can obtain a list of valid servers via: Server List.</param>
		/// <param name="page">Search or move to a specific page.
		/// There is currently no way to change the amount of results back returned.It will always be 50 per page with a maximum of 20 pages.This is due to how Lodestone works.</param>
		public static async Task<GetResponse> Search(string name, string? server = null, int page = 0)
		{
			string route = "/character/search?name=" + name;

			if (!string.IsNullOrEmpty(server))
				route += "&server=" + server;

			if (page != 0)
				route += "&page=" + page;

			return await Request.Send<GetResponse>(route);
		}

		/// <summary>
		/// Get Character data, this is parsed straight from Lodestone in real-time. The more data you request the slower the entire request will be.
		/// </summary>
		public static async Task<GetResponse> Get(uint? id, CharacterData dataFlags = CharacterData.None)
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

			return await Request.Send<GetResponse>("/character/" + id + "?data=" + data + "&extended=true");
		}

		[Serializable]
		public class SearchResponse
		{
			public Pagination? Pagination;
			public List<Member>? Results;
		}

		[Serializable]
		public class GetResponse
		{
			////public ? Achievements;
			public bool? AchievementsPublic;
			public Character? Character;
			////public ? FreeCompany;
			public List<Member>? FreeCompanyMembers;
			////public List<>? Friends;
			public bool? FriendsPublic;
			public uint? PvPTeam;
		}

		[Serializable]
		public class Character
		{
			public ClassJob? ActiveClassJob;
			public string? Avatar;
			public string? Bio;
			public List<ClassJob>? ClassJobs;
			public string? DC;
			public string? FreeCompanyId;
			////public ? GearSet;
			public uint? Gender;
			public GrandCompany? GrandCompany;
			public Data? GuardianDeity;
			public uint? ID;
			////public List<>? Minions;
			////public List<>? Mounts;
			public string? Name;
			public string? Nameday;
			public uint? ParseDate;
			public string? Portrait;
			public uint? PvPTeamId;
			public Data? Race;
			public string? Server;
			public Data? Title;
			public bool TitleTop;
			public Data? Town;
			public Data? Tribe;

			public override string? ToString()
			{
				return "Character: " + this.Name + " (" + this.ID + ")";
			}
		}

		[Serializable]
		public class Data
		{
			public int ID;
			public string? Name;
			public string? Icon;
			public string? Url;
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
		public class ClassJob
		{
			public Class? Class;
			public ulong ExpLevel = 0;
			public ulong ExpLevelMax = 0;
			public ulong ExpLevelTogo = 0;
			public bool IsSpecialised = false;
			public Class? Job;
			public int Level = 0;
			public string Name = string.Empty;
		}

		[Serializable]
		public class Class
		{
			public string? Abbreviation;
			//// public ? ClassJobCategory
			public uint? ID;
			public string? Icon;
			public string? Name;
			public string? Url;
		}

		[Serializable]
		public class GrandCompany
		{
			public uint? NameID;
			public uint? RankID;
		}
	}
}
