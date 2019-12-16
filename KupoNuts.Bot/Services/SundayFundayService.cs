// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Data;
	using KupoNuts.Utils;
	using NodaTime;
	using NodaTime.Calendars;

	public class SundayFundayService : ServiceBase
	{
		private const int NumberOfEvents = 4;

		private static readonly List<string> ReactionNames = new List<string>()
		{
			"🇦",
			"🇧",
			"🇨",
			"🇩",
			"🇪",
			"🇫",
			"🇬",
		};

		private static readonly List<IEmote> Reactions = new List<IEmote>()
		{
			new Emoji("🇦"),
			new Emoji("🇧"),
			new Emoji("🇨"),
			new Emoji("🇩"),
			new Emoji("🇪"),
			new Emoji("🇫"),
			new Emoji("🇬"),
		};

		private Table<SundayFundayEvent> database = Table<SundayFundayEvent>.Create("SundayFunday", 0);
		private ulong messageId;
		private Dictionary<ulong, List<string>> removingUserReactions = new Dictionary<ulong, List<string>>();

		public override async Task Initialize()
		{
			await this.database.Connect();

			await this.EnsureSingleVote();

			ScheduleService.RunOnSchedule(this.Update, 15);
			await this.Update();

			Program.DiscordClient.ReactionAdded += this.DiscordClient_ReactionAdded;
			Program.DiscordClient.ReactionRemoved += this.DiscordClient_ReactionRemoved;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.DiscordClient_ReactionAdded;
			Program.DiscordClient.ReactionRemoved -= this.DiscordClient_ReactionRemoved;
			return base.Shutdown();
		}

		[Command(@"SundayFunday", Permissions.Administrators, @"Updates the Sunday funday post")]
		public async Task Update()
		{
			(SocketTextChannel? channel, RestUserMessage? message) = await this.GetMessage();

			if (channel == null)
				return;

			(bool running, Duration timeTill) = this.GetTimeTill();

			bool needsAdvance = false;

			if (!running && timeTill.Days > 6 && timeTill.Hours > 23 && timeTill.Minutes > 30)
				needsAdvance = true;

			if (message == null || needsAdvance)
			{
				await this.AdvanceWeek();
				(channel, message) = await this.GetMessage();

				if (message == null)
				{
					throw new Exception(@"Failed to generate sunday funday message");
				}
			}

			Embed embed = await this.GenerateEmbed(message);

			if (message != null)
			{
				await message.ModifyAsync(x =>
				{
					x.Embed = embed;
				});

				if (running)
				{
					await message.RemoveAllReactionsAsync();
				}

				this.messageId = message.Id;
			}
		}

		[Command("SundayFundayAdvance", Permissions.Administrators, @"Advances Sunday funday by one week")]
		public async Task AdvanceWeek()
		{
			Log.Write(@"Advancing Sunday Funday Week", "Bot");

			SundayFundayEvent? lastWinner = null;
			int topVotes = 0;
			List<SundayFundayEvent> events = await this.database.LoadAll();

			(SocketTextChannel? channel, RestUserMessage? message) = await this.GetMessage();
			Dictionary<string, int> votes = await message.GetReactions();

			// Get the last winner, and reset the week for each event
			foreach (SundayFundayEvent evt in events)
			{
				if (!string.IsNullOrEmpty(evt.Reaction))
				{
					int voteCount = 0;
					if (votes.ContainsKey(evt.Reaction))
						voteCount = votes[evt.Reaction];

					if (voteCount > topVotes)
					{
						topVotes = voteCount;
						lastWinner = evt;
					}
				}

				evt.Reaction = string.Empty;
				await this.database.Save(evt);
			}

			// don't include last weeks winner as an option this week.
			if (lastWinner != null)
				events.Remove(lastWinner);

			// Choose new events to have up
			for (int i = 0; i < NumberOfEvents; i++)
			{
				SundayFundayEvent evt = events.GetRandom();
				evt.Reaction = ReactionNames[i];
				await this.database.Save(evt);
				events.Remove(evt);
			}

			if (message != null)
				await message.DeleteAsync();

			if (channel != null)
			{
				EmbedBuilder builder = new EmbedBuilder();
				builder.Description = "...";
				message = await channel.SendMessageAsync(null, false, builder.Build());
				this.messageId = message.Id;

				Settings settings = Settings.Load();
				settings.SundayFundayMessage = message.Id.ToString();
				settings.Save();

				for (int i = 0; i < NumberOfEvents; i++)
				{
					await message.AddReactionAsync(Reactions[i]);
				}
			}
		}

		private async Task<(SocketTextChannel?, RestUserMessage?)> GetMessage()
		{
			Settings settings = Settings.Load();

			ulong channelId = 0;
			ulong.TryParse(settings.SundayFundayChannel, out channelId);

			if (channelId == 0)
				return (null, null);

			SocketTextChannel? channel = Program.DiscordClient.GetChannel(channelId) as SocketTextChannel;

			if (channel == null)
				return (channel, null);

			ulong messageId = 0;
			ulong.TryParse(settings.SundayFundayMessage, out messageId);
			if (messageId == 0)
				return (channel, null);

			RestUserMessage? message = await channel.GetMessageAsync(messageId) as RestUserMessage;
			return (channel, message);
		}

		private Instant GetNextInstant(Instant from)
		{
			ZonedDateTime zdt = from.InZone(TimeUtils.Sydney);
			LocalDateTime ldt = zdt.LocalDateTime;
			ldt = new LocalDateTime(ldt.Year, ldt.Month, ldt.Day, 20, 00);

			if (ldt.DayOfWeek != IsoDayOfWeek.Sunday)
				ldt = ldt.Next(IsoDayOfWeek.Sunday);

			zdt = ldt.InZoneLeniently(TimeUtils.Sydney);
			return zdt.ToInstant();
		}

		private async Task EnsureSingleVote()
		{
			(SocketTextChannel? channel, RestUserMessage? message) = await this.GetMessage();

			if (message == null)
				return;

			Dictionary<ulong, (IUser user, List<IEmote> reactions)> votes = new Dictionary<ulong, (IUser user, List<IEmote> reactions)>();

			foreach ((IEmote emote, ReactionMetadata data) in message.Reactions)
			{
				IEnumerable<IUser> users = await message.GetReactionUsersAsync(emote, 999).FlattenAsync();
				foreach (IUser user in users)
				{
					if (user.Id == Program.DiscordClient.CurrentUser.Id)
						continue;

					if (!ReactionNames.Contains(emote.Name))
					{
						await message.RemoveReactionAsync(emote, user);
						continue;
					}

					if (!votes.ContainsKey(user.Id))
						votes.Add(user.Id, (user, new List<IEmote>()));

					votes[user.Id].reactions.Add(emote);
				}
			}

			foreach ((ulong userId, (IUser user, List<IEmote> emotes)) in votes)
			{
				if (emotes.Count > 1)
				{
					for (int i = 1; i < emotes.Count; i++)
					{
						await message.RemoveReactionAsync(emotes[i], user);
					}
				}
			}
		}

		private (bool running, Duration till) GetTimeTill()
		{
			Instant now = TimeUtils.Now;
			Instant next = this.GetNextInstant(now);
			Duration timeTill = next - TimeUtils.RoundInstant(now);

			bool running = timeTill.TotalSeconds <= 0;
			if (running)
			{
				next += Duration.FromHours(2);
				timeTill = next - TimeUtils.RoundInstant(now);
			}

			return (running, timeTill);
		}

		private async Task<Embed> GenerateEmbed(RestUserMessage message)
		{
			StringBuilder description = new StringBuilder();

			(bool running, Duration timeTill) = this.GetTimeTill();

			description.AppendLine(@"Vote for an event each week!");
			description.AppendLine();
			description.Append(running ? "Ends in " : "Starts in ");
			description.AppendLine(TimeUtils.GetDurationString(timeTill));
			description.AppendLine();

			List<SundayFundayEvent> events = await this.database.LoadAll();
			Dictionary<string, int> reactions = await message.GetReactions();

			SundayFundayEvent? winner = null;
			int topVotes = -1;

			events.Sort((a, b) =>
			{
				return a.Reaction.CompareTo(b.Reaction);
			});

			foreach (SundayFundayEvent evt in events)
			{
				if (string.IsNullOrEmpty(evt.Reaction))
					continue;

				int voteCount = 0;
				if (reactions.ContainsKey(evt.Reaction))
					voteCount = reactions[evt.Reaction] - 1;

				if (voteCount > topVotes)
				{
					topVotes = voteCount;
					winner = evt;
				}

				description.Append(evt.Reaction);
				description.Append(" - *");
				description.Append(voteCount.ToString());
				description.Append(" votes* - ");
				description.AppendLine(evt.Name);
			}

			if (winner != null)
			{
				description.AppendLine();
				description.Append("Current winner: __");
				description.Append(winner.Name);
				description.AppendLine("__");

				if (!string.IsNullOrEmpty(winner.Description))
					description.AppendLine(winner.Description);

				description.Append("*With ");
				description.Append(topVotes);
				description.AppendLine(" votes*");
			}

			description.AppendLine();
			description.Append("Don't want to do ");
			description.Append(winner?.Name);
			description.Append("? Vote now!");

			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = @"Sunday Funday";
			builder.Description = description.ToString();
			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = "Or don't; I'm a bot, not a cop.";
			return builder.Build();
		}

		private Task DiscordClient_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
		{
			if (arg1.Id != this.messageId)
				return Task.CompletedTask;

			if (!arg3.User.IsSpecified)
				return Task.CompletedTask;

			if (arg3.UserId == Program.DiscordClient.CurrentUser.Id)
				return Task.CompletedTask;

			IEmote emote = arg3.Emote;

			if (!ReactionNames.Contains(emote.Name))
				return Task.CompletedTask;

			if (this.removingUserReactions.ContainsKey(arg3.UserId))
			{
				if (this.removingUserReactions[arg3.UserId].Contains(emote.Name))
				{
					this.removingUserReactions[arg3.UserId].Remove(emote.Name);
					return Task.CompletedTask;
				}
			}

			_ = Task.Run(this.Update);
			return Task.CompletedTask;
		}

		private async Task DiscordClient_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
		{
			if (arg1.Id != this.messageId)
				return;

			if (!arg3.User.IsSpecified)
				return;

			IUser user = arg3.User.Value;

			if (user.Id == Program.DiscordClient.CurrentUser.Id)
				return;

			IEmote emote = arg3.Emote;
			IUserMessage msg = await arg1.GetOrDownloadAsync();

			// remove reacts that are not part of the current event lookup to prevent users
			// from adding their own reactions.
			if (!ReactionNames.Contains(arg3.Emote.Name))
			{
				await msg.RemoveReactionAsync(emote, user);
				return;
			}

			// find and remove any other reactions from this user, so they may only have one set
			// at any given time.
			foreach ((IEmote otherEmote, ReactionMetadata data) in msg.Reactions)
			{
				if (otherEmote.Name == emote.Name)
					continue;

				if (!this.removingUserReactions.ContainsKey(user.Id))
					this.removingUserReactions.Add(user.Id, new List<string>());

				this.removingUserReactions[user.Id].Add(otherEmote.Name);
				await msg.RemoveReactionAsync(otherEmote, user);
				await Task.Delay(100);
			}

			_ = Task.Run(this.Update);
		}
	}
}
