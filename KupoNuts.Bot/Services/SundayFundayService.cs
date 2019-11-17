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

			if (settings.SundayFundayWeek < 0)
				await this.AdvanceWeek();

			ulong channelId = 0;
			ulong messageId = 0;

			ulong.TryParse(settings.SundayFundayChannel, out channelId);
			ulong.TryParse(settings.SundayFundayMessage, out messageId);

			if (channelId == 0)
				return;

			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);

			Embed embed = await this.Generate();
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

				int votes = evt.CountVotes(lastWeek);
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
			LocalDateTime sundayLdt = ldt.Next(IsoDayOfWeek.Sunday);
			ZonedDateTime sundayZdt = sundayLdt.InZoneLeniently(TimeUtils.Sydney);
			return sundayZdt.ToInstant();
		}

		private async Task<Embed> Generate()
		{
			int currentWeek = Settings.Load().SundayFundayWeek;

			Instant now = TimeUtils.Now;
			Instant next = this.GetNextInstant(now);
			Duration timeTill = next - TimeUtils.RoundInstant(now);

			StringBuilder description = new StringBuilder();

			description.AppendLine("Its Sunday funday! Vote for an event each week!");
			description.AppendLine();
			description.Append("Starts in ");
			description.AppendLine(TimeUtils.GetDurationString(timeTill));
			description.AppendLine();

			List<SundayFundayEvent> events = await this.database.LoadAll();
			int count = 0;
			foreach (SundayFundayEvent evt in events)
			{
				if (evt.CurrentWeek != currentWeek)
					continue;

				int votes = evt.CountVotes(currentWeek);

				description.Append(ListEmotes[count]);
				description.Append(" - *");
				description.Append(votes.ToString());
				description.Append(" votes* - ");
				description.AppendLine(evt.Name);

				count++;
			}

			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = "Sunday Funday: Week " + currentWeek;
			builder.Description = description.ToString();
			return builder.Build();
		}

		private Task DiscordClient_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
		{
			if (arg1.Id != this.messageId)
				return Task.CompletedTask;

			if (!ListEmotes.Contains(arg3.Emote.Name))
				return Task.CompletedTask;

			return Task.CompletedTask;
		}
	}
}
