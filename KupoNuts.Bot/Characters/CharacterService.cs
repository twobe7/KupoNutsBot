// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using XIVAPI;

	public class CharacterService : ServiceBase
	{
		[Command("WhoIs", Permissions.Everyone, "looks up a character profile")]
		public async Task WhoIs(SocketMessage message, uint characterId)
		{
			CharacterAPI.GetResponse response = await CharacterAPI.Get(characterId);

			if (response.Character == null)
				throw new UserException("Sorry! That character was blank.");

			Embed embed = response.Character.BuildEmbed();
			await message.Channel.SendMessageAsync(null, false, embed);
		}
	}
}
