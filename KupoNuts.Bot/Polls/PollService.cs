// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Polls
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using NodaTime;

	public class PollService : ServiceBase
	{
		private Dictionary<ulong, Poll> pollLookup = new Dictionary<ulong, Poll>();

		private Database<Poll> pollDatabase = new Database<Poll>("Polls", 1);

		public override async Task Initialize()
		{
			await this.pollDatabase.Connect();

			CommandsService.BindCommand("poll", this.HandlePoll, Permissions.Administrators, "Copies a range of messages to a new channel, adds a reaction, and counts votes.");
			Program.DiscordClient.ReactionAdded += this.ReactionAdded;

			List<Poll> polls = await this.pollDatabase.LoadAll();

			foreach (Poll poll in polls)
			{
				bool valid = await poll.IsValid();

				if (!valid)
				{
					await this.pollDatabase.Delete(poll);
				}
				else
				{
					this.WatchPoll(poll);
				}
			}
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("poll");
			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;
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

			string comment = string.Empty;
			if (args.Length == 4)
				comment = args[3];

			Poll poll = await this.pollDatabase.CreateEntry();
			poll.ChannelId = message.Channel.Id.ToString();

			await ((SocketTextChannel)toChannel).SendMessageAsync(comment);
			List<RestUserMessage> messages = await EchoService.Echo((SocketTextChannel)message.Channel, (SocketTextChannel)toChannel, message.Id, count);

			poll.Options = new List<string>();
			foreach (RestUserMessage pollMessage in messages)
			{
				poll.Options.Add(pollMessage.Id.ToString());
				await pollMessage.AddReactionAsync(emote);
			}

			await this.pollDatabase.Save(poll);

			this.WatchPoll(poll);
		}

		private void WatchPoll(Poll poll)
		{
			if (poll.Options == null)
				return;

			foreach (string option in poll.Options)
			{
				if (!ulong.TryParse(option, out ulong messageID))
					continue;

				this.pollLookup.Add(messageID, poll);
			}
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!this.pollLookup.ContainsKey(message.Id))
				return;

			if (!reaction.User.IsSpecified)
				return;

			if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
				return;

			Poll poll = this.pollLookup[message.Id];

			if (poll.Options != null)
			{
				foreach (string optionIdStr in poll.Options)
				{
					if (!ulong.TryParse(optionIdStr, out ulong optionId))
						continue;

					if (optionId == message.Id)
						continue;

					IUserMessage option = (IUserMessage)await channel.GetMessageAsync(optionId);
					await option.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
				}
			}
		}
	}
}
