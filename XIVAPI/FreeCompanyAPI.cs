// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.API;

	public static class FreeCompanyAPI
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
			ClassJobs = 32,
		}

		/// <summary>
		/// Get Character data, this is parsed straight from Lodestone in real-time. The more data you request the slower the entire request will be.
		/// </summary>
		public static async Task<GetResponse> GetFreeCompany(ulong? id, CharacterData dataFlags = CharacterData.FreeCompanyMembers, string columns = "")
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

			if (FlagsUtils.IsSet(dataFlags, CharacterData.ClassJobs))
				data += "CJ,";

			return await Request.Send<GetResponse>("/freecompany/" + id + "?data=" + data + "&columns=" + columns);
		}

		public static async Task<GetResponse> GetCharacter(ulong? id, CharacterData dataFlags = CharacterData.None, string columns = "")
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

			if (FlagsUtils.IsSet(dataFlags, CharacterData.ClassJobs))
				data += "CJ,";

			return await Request.Send<GetResponse>("/character/" + id + "?data=" + data + "&columns=" + columns);
		}

		[Serializable]
		public class SearchResponse : ResponseBase
		{
			public Pagination? Pagination { get; set; }
			public List<Member>? Results { get; set; }
		}

		[Serializable]
		public class GetResponse : ResponseBase
		{
			public List<Member>? FreeCompanyMembers { get; set; }
			public Character? Character { get; set; }
		}

		[Serializable]
		public class Member
		{
			public uint ID { get; set; }
		}

		[Serializable]
		public class Character
		{
			public uint Gender { get; set; }
			public uint ID { get; set; }
			public uint ParseDate { get; set; }
			public uint Race { get; set; }
			public uint Tribe { get; set; }
		}
	}
}
