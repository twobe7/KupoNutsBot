// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Lodestone
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using global::Lodestone.News;
	using KupoNuts.Bot.Services;

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

		private async Task Update()
		{
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
					Log.Write("Posintg Lodestone news: " + item.title);
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
