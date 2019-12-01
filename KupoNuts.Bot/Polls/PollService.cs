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
	using KupoNuts.Utils;
	using NodaTime;

	public class PollService : ServiceBase
	{
		public static readonly List<string> ListEmotes = new List<string>()
		{
			"🇦",
			"🇧",
			"🇨",
			"🇩",
			"🇪",
			"🇫",
			"🇬",
		};

		private Dictionary<ulong, string> pollLookup = new Dictionary<ulong, string>();

		private Database<Poll> pollDatabase = new Database<Poll>("Polls", 2);

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
					await poll.UpdateMessage();
					this.WatchPoll(poll);
					await this.pollDatabase.Save(poll);
				}
			}
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;
			return Task.CompletedTask;
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, string comment, string a, string b)
		{
			return await this.SendPoll(message, (SocketTextChannel)message.Channel, comment, a, b, null, null);
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, string comment, string a, string b, SocketTextChannel channel)
		{
			return await this.SendPoll(message, channel, comment, a, b, null, null);
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, string comment, string a, string b, string c)
		{
			return await this.SendPoll(message, (SocketTextChannel)message.Channel, comment, a, b, c, null);
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, string comment, string a, string b, string c, SocketTextChannel channel)
		{
			return await this.SendPoll(message, channel, comment, a, b, c, null);
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, string comment, string a, string b, string c, string d)
		{
			return await this.SendPoll(message, (SocketTextChannel)message.Channel, comment, a, b, c, d);
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, string comment, string a, string b, string c, string d, SocketTextChannel channel)
		{
			return await this.SendPoll(message, channel, comment, a, b, c, d);
		}

		private async Task<bool> SendPoll(CommandMessage message, SocketTextChannel channel, string comment, string a, string b, string? c, string? d)
		{
			Poll poll = await this.pollDatabase.CreateEntry();
			poll.Comment = comment;
			poll.ChannelId = channel.Id;
			poll.ClosesInstant = TimeUtils.RoundInstant(TimeUtils.Now + Duration.FromDays(1));
			poll.Options.Add(new Poll.Option(a));
			poll.Options.Add(new Poll.Option(b));

			if (c != null)
				poll.Options.Add(new Poll.Option(c));

			if (d != null)
				poll.Options.Add(new Poll.Option(d));

			await poll.UpdateMessage();
			this.WatchPoll(poll);
			await this.pollDatabase.Save(poll);

			await message.Message.DeleteAsync();

			return true;
		}

		private void WatchPoll(Poll poll)
		{
			if (poll.Options == null)
				return;

			this.pollLookup.Add(poll.MessageId, poll.Id);
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				if (!this.pollLookup.ContainsKey(message.Id))
					return;

				if (!reaction.User.IsSpecified)
					return;

				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return;

				string pollId = this.pollLookup[message.Id];

				IUserMessage userMessage = await message.GetOrDownloadAsync();
				await userMessage.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

				int optionIndex = PollService.ListEmotes.IndexOf(reaction.Emote.Name);
				if (optionIndex < 0)
					return;

				Poll? poll = await this.pollDatabase.Load(pollId);
				if (poll == null)
					throw new Exception("Missing poll from database: \"" + pollId + "\"");

				poll.Vote(reaction.UserId, optionIndex);
				await this.pollDatabase.Save(poll);
				_ = Task.Run(poll.UpdateMessage);
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
