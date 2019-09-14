// This document is intended for use by Kupo Nut Brigade developers.

namespace Lodestone.News
{
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public static class NewsAPI
	{
		public static async Task<List<NewsItem>> Feed()
		{
			return await Request.Send<List<NewsItem>>("/news/feed");
		}

		public static async Task<List<NewsItem>> Latest(Categories category)
		{
			return await Request.Send<List<NewsItem>>("/news/" + category.ToString());
		}
	}
}
