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
			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;
			return Task.CompletedTask;
		}

		[Command("Poll", Permissions.Administrators, "Copies a range of messages as a poll.")]
		public async Task HandlePoll(SocketMessage message, IEmote emote, int count, string comment)
		{
			await this.HandlePoll(message, emote, count, (SocketTextChannel)message.Channel, comment);
		}

		[Command("Poll", Permissions.Administrators, "Copies a range of messages to a new channel as a poll.")]
		public async Task HandlePoll(SocketMessage message, IEmote emote, int count, SocketTextChannel channel, string comment)
		{
			Poll poll = await this.pollDatabase.CreateEntry();
			poll.ChannelId = message.Channel.Id.ToString();

			await channel.SendMessageAsync(comment);
			List<RestUserMessage> messages = await EchoService.Echo((SocketTextChannel)message.Channel, channel, message.Id, count);

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
