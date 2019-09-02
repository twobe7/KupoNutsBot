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
		public override Task Initialize()
		{
			CommandsService.BindCommand("whoIs", this.HandleWhoIs, Permissions.Everyone, string.Empty);
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			return Task.CompletedTask;
		}

		private async Task HandleWhoIs(string[] args, SocketMessage message)
		{
			if (args.Length != 1 || !uint.TryParse(args[0], out uint characterId))
			{
				await message.Channel.SendMessageAsync("Sorry! try \"WhoIs 17376038\"");
				return;
			}

			CharacterAPI.GetResponse response = await CharacterAPI.Get(characterId);

			if (response.Character == null)
			{
				await message.Channel.SendMessageAsync("Sorry! That character was blank.");
				return;
			}

			Embed embed = response.Character.BuildEmbed();
			await message.Channel.SendMessageAsync(null, false, embed);
		}
	}
}
