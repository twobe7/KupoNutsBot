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

			Scheduler.RunOnSchedule(this.UpdatePolls, 15);
			await this.UpdatePolls();
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;
			return Task.CompletedTask;
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b)
		{
			return await this.SendPoll(message, duration, comment, new List<string>() { a, b });
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c)
		{
			return await this.SendPoll(message, duration, comment, new List<string>() { a, b, c });
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c, string d)
		{
			return await this.SendPoll(message, duration, comment, new List<string>() { a, b, c, d });
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c, string d, string e)
		{
			return await this.SendPoll(message, duration, comment, new List<string>() { a, b, c, d, e });
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c, string d, string e, string f)
		{
			return await this.SendPoll(message, duration, comment, new List<string>() { a, b, c, d, e, f });
		}

		[Command("Poll", Permissions.Administrators, "Creates a poll")]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c, string d, string e, string f, string g)
		{
			return await this.SendPoll(message, duration, comment, new List<string>() { a, b, c, d, e, f, g });
		}

		private async Task UpdatePolls()
		{
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

		private async Task<bool> SendPoll(CommandMessage message, Duration duration, string comment, List<string> options)
		{
			Poll poll = await this.pollDatabase.CreateEntry();
			poll.Comment = comment;
			poll.ChannelId = message.Channel.Id;
			poll.ClosesInstant = TimeUtils.RoundInstant(TimeUtils.Now + duration);

			foreach (string op in options)
				poll.Options.Add(new Poll.Option(op));

			await this.UpdatePoll(poll);

			await message.Message.DeleteAsync();

			return true;
		}

		private void WatchPoll(Poll poll)
		{
			if (poll.Options == null)
				return;

			this.pollLookup.Add(poll.MessageId, poll.Id);
		}

		private async Task UpdatePoll(Poll poll)
		{
			if (poll.Closed())
			{
				await poll.Close();
				this.pollLookup.Remove(poll.MessageId);
				await this.pollDatabase.Delete(poll);
			}

			await poll.UpdateMessage();
			this.WatchPoll(poll);
			await this.pollDatabase.Save(poll);
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
				_ = Task.Run(async () => { await this.UpdatePoll(poll); });
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
