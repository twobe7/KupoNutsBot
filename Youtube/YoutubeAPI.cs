// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Youtube
{
	using Discord;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public class YoutubeAPI
	{
		public static async Task<YoutubeVideo> GetVideoInformation(string videoId)
		{
			YoutubeVideo video = await Request.GetYoutubeVideoInfo<YoutubeVideo>(videoId);

			return video;
		}

		public class YoutubeVideo : ResponseBase
		{
			public string Kind { get; set; }
			public string ETag { get; set; }
			public List<YoutubeItem> Items { get; set; }
		}

		public class YoutubeItem
		{
			public string Kind { get; set; }
			public string ETag { get; set; }
			public string Id { get; set; }
			public YoutubeSnippet Snippet { get; set; }
		}

		public class YoutubeSnippet
		{
			public DateTime PublishedAt { get; set; }
			public string ChannelId { get; set; }
			public string Title { get; set; }
			public string Description { get; set; }
			public Dictionary<string, Thumbnail> Thumbnails { get; set; }
			public string ChannelTitle { get; set; }
			public List<string> Tags { get; set; }
			public string CategoryId { get; set; }
			public string LiveBroadcastContent { get; set; }
		}

		public class Thumbnail
		{
			public string Url { get; set; }
			public uint Width { get; set; }
			public uint Height { get; set; }
		}
	}
}
