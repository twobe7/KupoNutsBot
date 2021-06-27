// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Twitter
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC;
	using TweetSharp;

	public static class FashionReportAPI
	{
		public static Task<List<FashionReportEntry>> Get()
		{
			Settings settings = Settings.Load();

			List<FashionReportEntry> results = new List<FashionReportEntry>();

			if (settings.TwitterConsumerKey == null || settings.TwitterConsumerSecret == null || settings.TwitterToken == null || settings.TwitterTokenSecret == null)
				return Task.FromResult(results);

			TwitterService service = new TwitterService(settings.TwitterConsumerKey, settings.TwitterConsumerSecret);
			service.AuthenticateWith(settings.TwitterToken, settings.TwitterTokenSecret);

			if (service == null)
				return Task.FromResult(results);

			ListTweetsOnUserTimelineOptions op = new ListTweetsOnUserTimelineOptions
			{
				ScreenName = "@KaiyokoStar",
				IncludeRts = false,
				ExcludeReplies = true,
			};

			IEnumerable<TwitterStatus> statuses = service.ListTweetsOnUserTimeline(op);

			foreach (TwitterStatus status in statuses)
			{
				if (status.Text == null)
					continue;

				if (!status.Text.Contains("Fashion Report Week"))
					continue;

				if (!status.Text.Contains("Full Details"))
					continue;

				FashionReportEntry entry = new FashionReportEntry
				{
					Id = status.IdStr,
					Time = status.CreatedDate,
					Content = status.Text,
					Author = status.Author.ScreenName,
					AuthorImageUrl = status.Author.ProfileImageUrl,
				};

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
