// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;

	public class PollService : ServiceBase
	{
		public override Task Initialize()
		{
			CommandsService.BindCommand("poll", this.HandlePoll, Permissions.Administrators, "Copies a range of messages to a new channel, adds a reaction, and counts votes.");
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("poll");
			return Task.CompletedTask;
		}

		private async Task HandlePoll(string[] args, SocketMessage message)
		{
			if (args.Length != 3 && args.Length != 4)
			{
				await message.Channel.SendMessageAsync("You need to tell me how many messages and what channel to copy to! try \"poll 5 :MSQDone: #channel-name \"Hello world!\"\" to copy the previous 5 message to a new channel and react with the MSQDone emote");
				return;
			}

			if (!int.TryParse(args[0], out int count) || count <= 0)
			{
				await message.Channel.SendMessageAsync("Bad message count: \"" + args[0] + "\"");
				return;
			}

			if (!Emote.TryParse(args[1], out Emote emote) || emote == null)
			{
				await message.Channel.SendMessageAsync("Bad emote count: \"" + args[1] + "\"");
				return;
			}

			SocketGuildChannel toChannel = message.MentionedChannels.Getfirst();
			if (toChannel == null || !(toChannel is SocketTextChannel))
			{
				await message.Channel.SendMessageAsync("Bad destination channel: \"" + args[2] + "\". Must be a text channel!");
				return;
			}

			if (args.Length == 4)
			{
				string comment = args[3];
				await ((SocketTextChannel)message.Channel).SendMessageAsync(comment);
			}

			List<RestUserMessage> messages = await EchoService.Echo((SocketTextChannel)message.Channel, (SocketTextChannel)toChannel, message.Id, count);

			foreach (RestUserMessage pollMessage in messages)
			{
				await pollMessage.AddReactionAsync(emote);
			}
		}
	}
}
