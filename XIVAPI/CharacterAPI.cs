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
		public static async Task<SearchResponse> Search(string name, string? server = null, int page = 0)
		{
			string route = "/character/search?name=" + name;

			if (!string.IsNullOrEmpty(server))
				route += "&server=" + server;

			if (page != 0)
				route += "&page=" + page;

			return await Request.Send<SearchResponse>(route);
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

		#pragma warning disable SA1516

		[Serializable]
		public class SearchResponse : ResponseBase
		{
			public Pagination? Pagination;
			public List<Member>? Results;
		}

		[Serializable]
		public class GetResponse : ResponseBase
		{
			////public ? Achievements;
			public bool? AchievementsPublic { get; set; }
			public Character? Character { get; set; }
			////public ? FreeCompany;
			public List<Member>? FreeCompanyMembers { get; set; }
			////public List<>? Friends;
			public bool? FriendsPublic { get; set; }
			public uint? PvPTeam { get; set; }
		}

		[Serializable]
		public class Character
		{
			public ClassJob? ActiveClassJob { get; set; }
			public string Avatar { get; set; } = string.Empty;
			public string Bio { get; set; } = string.Empty;
			public List<ClassJob>? ClassJobs { get; set; }
			public string DC { get; set; } = string.Empty;
			public string FreeCompanyId { get; set; } = string.Empty;
			////public ? GearSet;
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
			public uint? PvPTeamId { get; set; }
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

		[Serializable]
		public class Data
		{
			public int ID { get; set; }
			public string Name { get; set; } = string.Empty;
			public string Icon { get; set; } = string.Empty;
			public string Url { get; set; } = string.Empty;
		}

		[Serializable]
		public class Member
		{
			public string Avatar { get; set; } = string.Empty;
			public uint FeastMatches { get; set; }
			public uint ID { get; set; }
			public string Name { get; set; } = string.Empty;
			public string Rank { get; set; } = string.Empty;
			public string RankIcon { get; set; } = string.Empty;
			public string Server { get; set; } = string.Empty;

			public override string? ToString()
			{
				return "Member: " + this.Name + " (" + this.ID + ")";
			}
		}

		[Serializable]
		public class ClassJob
		{
			public Class? Class { get; set; }
			public ulong ExpLevel { get; set; } = 0;
			public ulong ExpLevelMax { get; set; } = 0;
			public ulong ExpLevelTogo { get; set; } = 0;
			public bool IsSpecialised { get; set; } = false;
			public Class? Job { get; set; }
			public int Level { get; set; } = 0;
			public string Name { get; set; } = string.Empty;
		}

		[Serializable]
		public class Class
		{
			public string Abbreviation { get; set; } = string.Empty;
			//// public ? ClassJobCategory
			public uint ID { get; set; }
			public string Icon { get; set; } = string.Empty;
			public string Name { get; set; } = string.Empty;
			public string Url { get; set; } = string.Empty;
		}

		[Serializable]
		public class GrandCompany
		{
			public uint NameID;
			public uint RankID;
		}
	}
}
