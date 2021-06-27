// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Lodestone
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Data;
	using FC.Utils;
	using global::Lodestone.News;
	using NodaTime;

	public class LodestoneService : ServiceBase
	{
		private Table<PostedNews> newsDb = new Table<PostedNews>("KupoNuts_News", 0);

		public override async Task Initialize()
		{
			await this.newsDb.Connect();
			await base.Initialize();

			await this.Update();
			ScheduleService.RunOnSchedule(this.Update, 15);
		}

		[Command("Maint", Permissions.Everyone, "Gets info about the next maintenance window.", CommandCategory.XIVData, "Maintenance")]
		[Command("Maintenance", Permissions.Everyone, "Gets info about the next maintenance window.", CommandCategory.XIVData)]
		public async Task GetMaintenance(CommandMessage message)
		{
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
				EmbedBuilder builder = new EmbedBuilder
				{
					ThumbnailUrl = "http://na.lodestonenews.com/images/maintenance.png",
					////ThumbnailUrl = "https://img.finalfantasyxiv.com/lds/h/F/DlQYVw2bqzA5ZOCfXKZ-Qe1IZU.svg",
					Title = nextMaint.Title,
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
				}
				else
				{
					builder.Description = "Completed: " + TimeUtils.GetDurationString(now - end) + " ago.";
				}

				////builder.AddField("Starts", TimeUtils.GetDateTimeString(start));
				////builder.AddField("Ends", TimeUtils.GetDateTimeString(end));
				builder.AddField("Starts", await TimeUtils.GetTimeList(message.Guild.Id, start));
				builder.AddField("Ends", await TimeUtils.GetTimeList(message.Guild.Id, end));
				builder.AddField("Duration", TimeUtils.GetDurationString(end - start));

				await message.Channel.SendMessageAsync(embed: builder.Build(), messageReference: message.MessageReference);
				return;
			}

			throw new UserException("I couldn't find any maintenance.");
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

				if (!entry.IsPosted)
				{
					Log.Write("Posting Lodestone news: " + item.Title, "Bot");
					await item.Post(channelId);

					entry.IsPosted = true;
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

			foreach (SocketGuild guild in Program.DiscordClient.Guilds)
			{
				GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guild.Id);

				if (!ulong.TryParse(settings.LodestoneChannel, out ulong channelId))
					return;

				foreach (NewsItem item in news)
				{
					if (item.Id == null)
						continue;

					PostedNews entry = await this.newsDb.LoadOrCreate(item.Id);

					if (!entry.IsPosted)
					{
						if (item.Description == null && item.Url != null)
							item.Description = await NewsAPI.Detail(item.Url);

						Log.Write("Posting Lodestone news: " + item.Title, "Bot");
						await item.Post(channelId);

						entry.IsPosted = true;
						await this.newsDb.Save(entry);

						// don't flood channel!
						await Task.Delay(500);
					}
				}
			}
		}

		public class PostedNews : EntryBase
		{
			public bool IsPosted { get; set; }
		}
	}
}
