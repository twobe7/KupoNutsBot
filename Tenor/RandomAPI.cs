// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Tenor
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public static class RandomAPI
	{
		public static async Task<Result> Random(string name)
		{
			string route = "/random?q=" + name;

			// Provide a content filter to filter out R content
			route += "&contentfilter=low";

			// Restrict some content types
			route += "&media_filter=minimal";

			// We only want a single result to limit data coming back
			route += "&limit=1";

			// For some reason the random endpoint always returns the same gif despite being random
			// Generate random int and return result at that position
			route += "&pos=" + new Random().Next();

			SearchResponse response = await Request.Send<SearchResponse>(route);

			if (response.Results == null)
				return new Result();

			return response.Results[0];
		}

		[Serializable]
		public class SearchResponse : ResponseBase
		{
			public string? Next { get; set; }
			public List<Result>? Results { get; set; }
		}

		[Serializable]
		public class Result
		{
			public List<string>? Tags { get; set; }
			public string Url { get; set; }
			public List<Dictionary<string, MediaItem>>? Media { get; set; }
			public decimal? Created { get; set; }
			public uint? Shares { get; set; }
			public string? ItemUrl { get; set; }
			public string? Composite { get; set; }
			public bool? HasAudio { get; set; }
			public string? Title { get; set; }
			public string? Id { get; set; }
			
			public string GetBestUrl()
			{
				string url = string.Empty;

				if (Media != null && Media.Count > 0)
				{
					Dictionary<string, MediaItem> media = Media[0];

					if (media.ContainsKey("tinygif"))
					{
						url = media["tinygif"].Url;
					}
					else if(media.ContainsKey("gif"))
					{
						url = media["gif"].Url;
					}
				}

				return url;
			}
		}

		[Serializable]
		public class Media
		{
			public MediaItem? Gif { get; set; }
			public MediaItem? MP4 { get; set; }
			public MediaItem? TinyGif { get; set; }
		}

		[Serializable]
		public class MediaItem
		{
			public string? Url { get; set; }
			public List<int>? Dims { get; set; }
			public decimal? Duration { get; set; }
			public string? Preview { get; set; }
			public ulong? Size { get; set; }
		}
	}
}
