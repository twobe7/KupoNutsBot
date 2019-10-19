// This document is intended for use by Kupo Nut Brigade developers.

namespace FFXIVCollect
{
	using System;
	using System.Threading.Tasks;

	public static class CharacterAPI
	{
		public static async Task<Character?> Get(uint id)
		{
			try
			{
				return await Request.Send<Character>("/characters/" + id);
			}
			catch (Exception)
			{
				// TODO: only catch 404's here...
				return null;
			}
		}

		#pragma warning disable SA1516

		[Serializable]
		public class Character
		{
			public uint Id { get; set; }
			public string Name { get; set; } = string.Empty;
			public string Server { get; set; } = string.Empty;
			public string Portrait { get; set; } = string.Empty;
			public string Avatar { get; set; } = string.Empty;
			public string Last_parsed { get; set; } = string.Empty;
			public bool Verified { get; set; } = false;
			public Data? Achievements { get; set; }
			public Data? Mounts { get; set; }
			public Data? Minions { get; set; }
			public Data? Orchestrions { get; set; }
			public Data? Emotes { get; set; }
			public Data? Bardings { get; set; }
			public Data? Hairstyles { get; set; }
			public Data? Armoires { get; set; }
			public Data? Triad { get; set; }
		}

		[Serializable]
		public class AchievementsData : Data
		{
			public int Points { get; set; }
			public int Points_Total { get; set; }
			public bool Public { get; set; }
		}

		[Serializable]
		public class Data
		{
			public int Count { get; set; }
			public int Total { get; set; }
		}
	}
}
