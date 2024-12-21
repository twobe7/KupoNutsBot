// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Lodestone
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Data;
	using FC.Utils;
	using global::Lodestone.News;
	using NodaTime;

	public class LodestoneService : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient;

		private readonly Table<PostedNews> newsDb = new Table<PostedNews>("KupoNuts_News", 0);

		public LodestoneService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		public override async Task Initialize()
		{
			await this.newsDb.Connect();
			await base.Initialize();

			await this.Update();
			ScheduleService.RunOnSchedule(this.Update, 15);
		}

		[SlashCommand("maintenance", "Gets information about the next maintenance window.")]
		public async Task GetMaintenance()
		{
			await this.DeferAsync();

			List<NewsItem> items = await NewsAPI.Latest(Categories.Maintenance);

			Instant now = TimeUtils.Now;
			NewsItem? nextMaint = null;
			Instant? bestStart = null;
			foreach (NewsItem item in items)
			{
				Instant? start = item.GetStart();
				Instant? end = item.GetEnd();

				if (start == null || end == null)
					continue;

				if (!item.Title.Contains("All Worlds"))
					continue;

				if (start < bestStart)
					continue;

				if (start < now.Minus(Duration.FromDays(14)))
					continue;

				bestStart = start;
				nextMaint = item;
			}

			if (nextMaint != null)
			{
				EmbedBuilder builder = new()
				{
					ThumbnailUrl = "http://na.lodestonenews.com/images/maintenance.png",
					Title = nextMaint.Title,
					Url = nextMaint.Url,
				};

				Instant? start = nextMaint.GetStart();
				Instant? end = nextMaint.GetEnd();

				if (start == null || end == null)
					throw new Exception();

				Duration timeUntilStart = (Duration)(start - now);
				Duration timeUntilEnd = (Duration)(end - now);

				if (timeUntilStart.TotalMinutes > 0)
				{
					builder.Description = "Starts In: " + TimeUtils.GetDurationString(start - now);
				}
				else if (timeUntilEnd.TotalMinutes > 0)
				{
					builder.Description = "Ends In: " + TimeUtils.GetDurationString(end - now);
					builder.ThumbnailUrl = "http://na.lodestonenews.com/images/status.png";
				}
				else
				{
					builder.Description = "Completed: " + TimeUtils.GetDurationString(now - end) + " ago.";
				}

				builder.AddField("Starts", TimeUtils.GetDiscordTimestamp(start.GetValueOrDefault().ToUnixTimeSeconds()));
				builder.AddField("Ends", TimeUtils.GetDiscordTimestamp(end.GetValueOrDefault().ToUnixTimeSeconds()));
				builder.AddField("Duration", TimeUtils.GetDurationString(end - start) ?? "Unknown");

				await this.FollowupAsync(embed: builder.Build());
				return;
			}

			await this.FollowupAsync("I couldn't find any maintenance.");
		}

		[Command("News", Permissions.Administrators, "Updates lodestone news")]
		public async Task News(CommandMessage message)
		{
			Log.Write("Updating lodestone news for guild: " + message.Guild.Name, "Bot");

			List<NewsItem> news = await NewsAPI.Feed();
			news.Reverse();

			GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(message.Guild.Id);

			if (!ulong.TryParse(settings.LodestoneChannel, out ulong channelId))
				return;

			foreach (NewsItem item in news)
			{
				if (item.Id == null)
					continue;

				PostedNews entry = await this.newsDb.LoadOrCreate(item.Id);

				if (!entry.PostedGuildIdList.Contains(message.Guild.Id))
				{
					Log.Write("Posting Lodestone news: " + item.Title, "Bot");
					await item.Post(channelId);

					entry.IsPosted = true;
					entry.PostedGuildIdList.Add(message.Guild.Id);
					await this.newsDb.Save(entry);

					// don't flood channel!
					await Task.Delay(500);
				}
			}
		}

		public async Task Update()
		{
			Log.Write("Updating lodestone news for all guilds", "Bot");

			List<NewsItem> news = await NewsAPI.Feed();
			news.Reverse();

#if DEBUG
			news = new List<NewsItem>();
#endif

			Dictionary<ulong, ulong> guildLodestoneChannel = new Dictionary<ulong, ulong>();

			foreach (SocketGuild guild in this.DiscordClient.Guilds)
			{
				GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guild.Id);
				if (ulong.TryParse(settings.LodestoneChannel, out ulong channelId))
					guildLodestoneChannel.Add(guild.Id, channelId);
			}

			foreach (NewsItem item in news)
			{
				if (item.Id == null)
					continue;

				// Get entry from DB
				PostedNews entry = await this.newsDb.LoadOrCreate(item.Id);

				bool updated = false;

				foreach (KeyValuePair<ulong, ulong> guild in guildLodestoneChannel)
				{
					if (!entry.PostedGuildIdList.Contains(guild.Key))
					{
						if (item.Description == null && item.Url != null)
							item.Description = await NewsAPI.Detail(item.Url);

						Log.Write($"Posting Lodestone news for {guild.Key}: {item.Title}", "Bot");
						await item.Post(guild.Value);

						entry.IsPosted = true;
						entry.PostedGuildIdList.Add(guild.Key);
						updated = true;
					}
				}

				// don't flood channel!
				await Task.Delay(500);

				if (updated)
					await this.newsDb.Save(entry);
			}
		}

		public class PostedNews : EntryBase
		{
			public bool IsPosted { get; set; }
			public List<ulong> PostedGuildIdList { get; set; } = new List<ulong>();
		}
	}
}
