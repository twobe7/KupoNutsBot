// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using KupoNuts.Characters;

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
			public FreeCompany? FreeCompany { get; set; }
			public List<Member>? FreeCompanyMembers { get; set; }
			////public List<>? Friends;
			public bool? FriendsPublic { get; set; }
			public uint? PvPTeam { get; set; }
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
	}
}
