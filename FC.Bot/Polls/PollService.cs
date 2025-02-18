﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Polls
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Polls;
	using FC.Bot.Services;
	using FC.Data;
	using FC.Utils;
	using NodaTime;

	public class PollService(DiscordSocketClient discordClient) : ServiceBase
	{
		public static readonly List<string> ListEmotes = [
			"🇦",
			"🇧",
			"🇨",
			"🇩",
			"🇪",
			"🇫",
			"🇬",
		];

		public readonly DiscordSocketClient DiscordClient = discordClient;

		private Dictionary<ulong, string> pollLookup = [];

		private Table<FC.Poll> pollDatabase = new("KupoNuts_Polls", 2);

		public override async Task Initialize()
		{
			await this.pollDatabase.Connect();

			this.DiscordClient.ReactionAdded += this.ReactionAdded;

			ScheduleService.RunOnSchedule(this.UpdatePolls, 15);
			await this.UpdatePolls();
		}

		public override Task Shutdown()
		{
			this.DiscordClient.ReactionAdded -= this.ReactionAdded;
			return Task.CompletedTask;
		}

		[Command("Poll", Permissions.Everyone, "Creates a poll", requiresQuotes: true)]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b)
		{
			return await this.SendPoll(message, duration, comment, [a, b]);
		}

		[Command("Poll", Permissions.Everyone, "Creates a poll", requiresQuotes: true)]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c)
		{
			return await this.SendPoll(message, duration, comment, [a, b, c]);
		}

		[Command("Poll", Permissions.Everyone, "Creates a poll", requiresQuotes: true)]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c, string d)
		{
			return await this.SendPoll(message, duration, comment, [a, b, c, d]);
		}

		[Command("Poll", Permissions.Everyone, "Creates a poll", requiresQuotes: true)]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c, string d, string e)
		{
			return await this.SendPoll(message, duration, comment, [a, b, c, d, e]);
		}

		[Command("Poll", Permissions.Everyone, "Creates a poll", requiresQuotes: true)]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c, string d, string e, string f)
		{
			return await this.SendPoll(message, duration, comment, [a, b, c, d, e, f]);
		}

		[Command("Poll", Permissions.Everyone, "Creates a poll", requiresQuotes: true)]
		public async Task<bool> HandlePoll(CommandMessage message, Duration duration, string comment, string a, string b, string c, string d, string e, string f, string g)
		{
			return await this.SendPoll(message, duration, comment, [a, b, c, d, e, f, g]);
		}

		[Command("ClosePoll", Permissions.Everyone, "Immediately closes a poll. Only the poll author or administrators can do this.")]
		public async Task ClosePoll(CommandMessage message, ulong messageId)
		{
			if (!this.pollLookup.TryGetValue(messageId, out string? pollId))
				throw new UserException("I couldn't find that poll");

			FC.Poll? poll = await this.pollDatabase.Load(pollId)
				?? throw new Exception("Poll missing from database: \"" + pollId + "\"");

			if (CommandsService.GetPermissions(message.Author) == Permissions.Everyone)
			{
				if (poll.Author != message.Author.Id)
				{
					throw new UserException("I'm sorry, only the polls author, or an administrator can do that.");
				}
			}

			await poll.Close();
			this.pollLookup.Remove(poll.MessageId);
			await this.pollDatabase.Delete(poll);
			await poll.UpdateMessage();
		}

		[Command("Polls", Permissions.Administrators, "Updates all active polls")]
		public async Task UpdatePolls()
		{
			List<FC.Poll> polls = await this.pollDatabase.LoadAll();
			foreach (FC.Poll poll in polls)
			{
				bool valid = await poll.IsValid();
				if (!valid)
				{
					await this.pollDatabase.Delete(poll);
				}
				else
				{
					await this.UpdatePoll(poll);
				}
			}
		}

		private async Task<bool> SendPoll(CommandMessage message, Duration duration, string comment, List<string> options)
		{
			if (duration < Duration.FromMinutes(1))
				throw new UserException("Polls must run for at least 1 minute");

			FC.Poll poll = new()
			{
				Id = Guid.NewGuid().ToString(),
				Author = message.Author.Id,
				Comment = comment,
				ChannelId = message.Channel.Id,
				ClosesInstant = TimeUtils.Now + duration,
			};

			await this.pollDatabase.Save(poll);

			foreach (string op in options)
				poll.Options.Add(new FC.Poll.Option(op));

			await this.UpdatePoll(poll);

			await message.DeleteMessage();

			return true;
		}

		private void WatchPoll(FC.Poll poll)
		{
			if (poll.Options == null)
				return;

			if (this.pollLookup.ContainsKey(poll.MessageId))
				this.pollLookup.Remove(poll.MessageId);

			this.pollLookup.Add(poll.MessageId, poll.Id);
		}

		private async Task UpdatePoll(FC.Poll poll)
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

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			try
			{
				if (!this.pollLookup.ContainsKey(message.Id))
					return;

				if (!reaction.User.IsSpecified)
					return;

				if (reaction.UserId == this.DiscordClient.CurrentUser.Id)
					return;

				string pollId = this.pollLookup[message.Id];

				IUserMessage userMessage = await message.GetOrDownloadAsync();
				await userMessage.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

				int optionIndex = PollService.ListEmotes.IndexOf(reaction.Emote.Name);
				if (optionIndex < 0)
					return;

				FC.Poll? poll = await this.pollDatabase.Load(pollId)
					?? throw new Exception("Missing poll from database: \"" + pollId + "\"");

				if (!poll.Closed())
				{
					poll.Vote(reaction.UserId, optionIndex);
					await this.pollDatabase.Save(poll);
				}

				_ = Task.Run(async () => { await this.UpdatePoll(poll); });
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
