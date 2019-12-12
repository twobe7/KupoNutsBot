// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;

	public class ModerationService : ServiceBase
	{
		[Command("Clear", Permissions.Administrators, "Clears a range of messages from the channel")]
		public async Task Clear(CommandMessage message, int count)
		{
			IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync(count + 1).FlattenAsync();

			foreach (IMessage messageToRemove in messages)
			{
				await messageToRemove.DeleteAsync();
				await Task.Delay(100);
			}
		}

		[Command("Ban", Permissions.Administrators, "Bans a user")]
		public async Task Ban(CommandMessage message, IGuildUser user)
		{
			await message.Guild.AddBanAsync(user);
		}

		[Command("Ban", Permissions.Administrators, "Bans a user")]
		public async Task Ban(CommandMessage message, ulong id)
		{
			IUser user = await message.Guild.GetUserAsync(id);

			if (user == null)
				throw new UserException("I couldn't find that user.");

			await message.Guild.AddBanAsync(user);
		}
	}
}
