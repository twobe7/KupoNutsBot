// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Lodestone
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using global::Lodestone.News;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using KupoNuts.Utils;
	using NodaTime;

	public class LodestoneService : ServiceBase
	{
		private Database<PostedNews> newsDb = new Database<PostedNews>("News", 0);

		public override async Task Initialize()
		{
			await this.newsDb.Connect();
			await base.Initialize();

			await this.Update();
			Scheduler.RunOnSchedule(this.Update, 15);
		}

		[Command("maintenance", Permissions.Everyone, "Gets info about the next maintanance window.")]
		[Command("maint", Permissions.Everyone, "Gets info about the next maintanance window.")]
		public async Task<Embed> GetMaintenance()
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

				if (!item.title.Contains("All Worlds"))
					continue;

				if (start < bestStart)
					continue;

				bestStart = start;
				nextMaint = item;
			}

			if (nextMaint != null)
			{
				EmbedBuilder builder = new EmbedBuilder();
				builder.ThumbnailUrl = "https://img.finalfantasyxiv.com/lds/h/F/DlQYVw2bqzA5ZOCfXKZ-Qe1IZU.svg";
				builder.Title = nextMaint.title;

				Instant? start = nextMaint.GetStart();
				Instant? end = nextMaint.GetEnd();

				if (start == null || end == null)
					throw new Exception();

				Duration timeTillStart = (Duration)(start - now);

				if (timeTillStart.TotalMinutes > 0)
				{
					builder.Description = "Starts In: " + TimeUtils.GetDurationString(start - now);
				}
				else
				{
					builder.Description = "Ends In: " + TimeUtils.GetDurationString(end - now);
				}

				builder.AddField("Starts", TimeUtils.GetDateTimeString(start));
				builder.AddField("Ends", TimeUtils.GetDateTimeString(end));
				builder.AddField("Duration", TimeUtils.GetDurationString(end - start));
				return builder.Build();
			}

			throw new UserException("I couldn't find any maintanance.");
		}

		private async Task Update()
		{
			Log.Write("Updating lodestone news", "Bot");

			if (!ulong.TryParse(Settings.Load().LodestoneChannel, out ulong channelId))
				return;

			List<NewsItem> news = await NewsAPI.Feed();
			news.Reverse();
			foreach (NewsItem item in news)
			{
				if (item.id == null)
					continue;

				PostedNews entry = await this.newsDb.LoadOrCreate(item.id);

				if (!entry.IsPosted)
				{
					Log.Write("Posintg Lodestone news: " + item.title, "Bot");
					await item.Post(channelId);

					entry.IsPosted = true;
					await this.newsDb.Save(entry);

					// dont flood channel!
					await Task.Delay(500);
				}
			}
		}

		public class PostedNews : EntryBase
		{
			public bool IsPosted;
		}
	}
}
