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
	using KupoNuts.Utils;
	using NodaTime;
	using NodaTime.Calendars;

	public class SundayFundayService : ServiceBase
	{
		private const int NumberOfEvents = 4;

		private static readonly List<string> ListEmotes = new List<string>()
		{
			"🇦",
			"🇧",
			"🇨",
			"🇩",
			"🇪",
			"🇫",
			"🇬",
		};

		private Database<SundayFundayEvent> database = new Database<SundayFundayEvent>("SundayFunday", 0);
		private ulong messageId;
		private Dictionary<string, string> reactionEventLookup = new Dictionary<string, string>();

		public override async Task Initialize()
		{
			await this.database.Connect();
			Scheduler.RunOnSchedule(this.Update, 15);
			await this.Update();

			Program.DiscordClient.ReactionAdded += this.DiscordClient_ReactionAdded;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.DiscordClient_ReactionAdded;
			return base.Shutdown();
		}

		[Command("SundayFunday", Permissions.Administrators, "Updates the sunday funday post")]
		public async Task Update()
		{
			Log.Write("Updating Sunday Funday", "Bot");

			Settings settings = Settings.Load();

			Instant now = TimeUtils.Now;
			Instant next = this.GetNextInstant(now);
			Duration timeTill = next - TimeUtils.RoundInstant(now);

			if (settings.SundayFundayWeek < 0 && timeTill.TotalSeconds < -(60 * 60 * 2))
				await this.AdvanceWeek();

			ulong channelId = 0;
			ulong messageId = 0;

			ulong.TryParse(settings.SundayFundayChannel, out channelId);
			ulong.TryParse(settings.SundayFundayMessage, out messageId);

			if (channelId == 0)
				return;

			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);

			Embed embed = await this.Generate(timeTill);
			RestUserMessage? message = null;

			if (messageId != 0)
				message = (RestUserMessage)await channel.GetMessageAsync(messageId);

			if (message == null)
			{
				message = await channel.SendMessageAsync(null, false, embed);
				settings = Settings.Load();
				settings.SundayFundayMessage = message.Id.ToString();
				settings.Save();

				for (int i = 0; i < NumberOfEvents; i++)
				{
					await message.AddReactionAsync(new Emoji(ListEmotes[i]));
				}
			}
			else
			{
				await message.ModifyAsync(x =>
				{
					x.Embed = embed;
				});
			}

			this.messageId = message.Id;
		}

		[Command("SundayFundayAdvance", Permissions.Administrators, "Advances sunday funday by one week post")]
		public async Task AdvanceWeek()
		{
			Log.Write("Advancing Sunday Funday Week", "Bot");

			Settings settings = Settings.Load();
			int lastWeek = settings.SundayFundayWeek;
			settings.SundayFundayWeek++;
			int currentWeek = settings.SundayFundayWeek;
			settings.Save();

			SundayFundayEvent? lastWinner = null;
			int topVotes = 0;
			List<SundayFundayEvent> events = await this.database.LoadAll();

			// Get the last winner, and reset the week for each event
			foreach (SundayFundayEvent evt in events)
			{
				if (evt.CurrentWeek != lastWeek)
					continue;

				int votes = evt.Votes.Count;
				if (votes > topVotes)
				{
					topVotes = votes;
					lastWinner = evt;
				}

				evt.CurrentWeek = -1;
			}

			if (lastWinner != null)
				events.Remove(lastWinner);

			// Choose new events to have up
			for (int i = 0; i < NumberOfEvents; i++)
			{
				SundayFundayEvent evt = events.GetRandom();
				evt.CurrentWeek = currentWeek;
				await this.database.Save(evt);
				events.Remove(evt);
			}
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

		private async Task<Embed> Generate(Duration timeTill)
		{
			int currentWeek = Settings.Load().SundayFundayWeek;

			bool running = timeTill.TotalSeconds <= 0;
			if (running)
			{
				Instant now = TimeUtils.Now;
				Instant next = this.GetNextInstant(now);
				next += Duration.FromHours(2);
				timeTill = next - TimeUtils.RoundInstant(now);
			}

			StringBuilder description = new StringBuilder();

			description.AppendLine("Its Sunday funday! Vote for an event each week!");
			description.AppendLine();
			description.Append(running ? "Ends in " : "Starts in ");
			description.AppendLine(TimeUtils.GetDurationString(timeTill));
			description.AppendLine();

			this.reactionEventLookup.Clear();

			List<SundayFundayEvent> events = await this.database.LoadAll();
			int count = 0;
			SundayFundayEvent? winner = null;
			int topVotes = -1;
			foreach (SundayFundayEvent evt in events)
			{
				if (evt.CurrentWeek != currentWeek)
					continue;

				int votes = evt.Votes.Count;
				string str = ListEmotes[count];
				this.reactionEventLookup.Add(str, evt.Id);

				if (votes > topVotes)
				{
					topVotes = votes;
					winner = evt;
				}

				description.Append(str);
				description.Append(" - *");
				description.Append(votes.ToString());
				description.Append(" votes* - ");
				description.AppendLine(evt.Name);

				count++;
			}

			description.AppendLine();
			description.Append("Dont want to do ");
			description.Append(winner?.Name);
			description.Append("? Vote now!");

			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = "Sunday Funday: " + winner?.Name + " (" + topVotes + " votes)";
			builder.Description = description.ToString();
			return builder.Build();
		}

		private async Task DiscordClient_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
		{
			if (arg1.Id != this.messageId)
				return;

			if (!arg3.User.IsSpecified)
				return;

			if (arg3.UserId == Program.DiscordClient.CurrentUser.Id)
				return;

			IUserMessage msg = await arg1.GetOrDownloadAsync();
			await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);

			if (!this.reactionEventLookup.ContainsKey(arg3.Emote.Name))
				return;

			string eventId = this.reactionEventLookup[arg3.Emote.Name];
			SundayFundayEvent? evt = await this.database.Load(eventId);

			if (evt == null)
				throw new Exception("Missing event id: \"" + eventId + "\"");

			string userId = arg3.UserId.ToString();

			////if (evt.HasVoted(userId))
			////	return;

			// Remove other votes
			List<SundayFundayEvent> events = await this.database.LoadAll();
			foreach (SundayFundayEvent otherEvent in events)
			{
				if (otherEvent.RemoveVote(userId))
				{
					await this.database.Save(otherEvent);
				}
			}

			if (evt.HasVoted(userId))
				return;

			// apply this vote
			evt.AddVote(userId);
			await this.database.Save(evt);

			// update post
			_ = Task.Run(this.Update);
		}
	}
}
