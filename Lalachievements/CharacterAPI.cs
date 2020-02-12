// This document is intended for use by Kupo Nut Brigade developers.

namespace Lalachievements
{
	using System;
	using System.Threading.Tasks;

	public static class CharacterAPI
	{
		public static async Task<Character?> Get(uint id)
		{
			Character character = await Request.Send<Character>("/characters/" + id);

			if (character.Status == "Adding")
			{
				await Task.Delay(1000);
				return await Get(id);
			}

			return character;
		}

		#pragma warning disable SA1516
		[Serializable]
		public class Character
		{
			public uint Id { get; set; }
			public string Name { get; set; } = string.Empty;
			public string Status { get; set; } = string.Empty;
		}
	}
}
