// This document is intended for use by Kupo Nut Brigade developers.

namespace Twitter
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using KupoNuts;
	using TweetSharp;

	public static class FashionReportAPI
	{
		public static Task<List<FashionReportEntry>> Get()
		{
			Settings settings = Settings.Load();

			TwitterService service = new TwitterService(settings.TwitterConsumerKey, settings.TwitterConsumerSecret);
			service.AuthenticateWith(settings.TwitterToken, settings.TwitterTokenSecret);

			ListTweetsOnUserTimelineOptions op = new ListTweetsOnUserTimelineOptions();
			op.ScreenName = "@KaiyokoStar";
			op.IncludeRts = false;
			op.ExcludeReplies = true;
			IEnumerable<TwitterStatus> statuses = service.ListTweetsOnUserTimeline(op);

			List<FashionReportEntry> results = new List<FashionReportEntry>();
			foreach (TwitterStatus status in statuses)
			{
				if (!status.Text.Contains("Fashion Report Week"))
					continue;

				if (!status.Text.Contains("Full Details"))
					continue;

				FashionReportEntry entry = new FashionReportEntry();
				entry.Id = status.IdStr;
				entry.Time = status.CreatedDate;
				entry.Content = status.Text;
				entry.Author = status.Author.ScreenName;
				entry.AuthorImageUrl = status.Author.ProfileImageUrl;

				if (status.Entities != null && status.Entities.Media != null && status.Entities.Media.Count > 0)
				{
					entry.ImageUrl = status.Entities.Media[0].MediaUrl;
				}

				results.Add(entry);
			}

			return Task.FromResult(results);
		}
	}
}
