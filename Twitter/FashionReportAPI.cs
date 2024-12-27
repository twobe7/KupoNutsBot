// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Twitter
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text.Json;
	using System.Threading.Tasks;
	using FC;
	using FC.Serialization;

	public static class FashionReportAPI
	{
		private const string TwitterApiUsersUri = "https://api.twitter.com/2/users";
		private const string KaiyokoStarId = "4826718276";
		private const string TweetsUri = $"{TwitterApiUsersUri}/{KaiyokoStarId}/tweets";

		public static async Task<FashionReportEntry?> GetLatest(string? sinceId = null)
		{
			Settings settings = Settings.Load();

			if (settings.TwitterBearerToken == null)
				return null;

			// Create query
			var query = $"{TweetsUri}?start_time={DateTime.Now.Date.AddDays(-8).ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}";
			if (sinceId != null)
				query += $"&since_id={sinceId}";

			query += "&exclude=retweets,replies";
			query += "&tweet.fields=created_at";
			query += "&expansions=author_id,attachments.media_keys";
			query += "&user.fields=username,profile_image_url";
			query += "&media.fields=url";

			using HttpClient client = new();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.TwitterBearerToken}");

			try
			{
				var stream = await client.GetStreamAsync(query);
				using StreamReader reader = new(stream);
				string json = await reader.ReadToEndAsync();

				Log.Write($"Response: {json.Length} characters", "Twitter");

				var response = JsonSerializer.Deserialize<Response>(json, Serializer.SnakeCaseOptions);
				var data = response?.Data;

				if (data != null)
				{
					var user = response?.Includes.Users.FirstOrDefault();

					foreach (var result in data)
					{
						if (!result.Content.Contains("Fashion Report Week", StringComparison.InvariantCultureIgnoreCase))
							continue;

						if (!result.Content.Contains("Full Details", StringComparison.InvariantCultureIgnoreCase))
							continue;

						// Update with Includes data
						if (user != null)
						{
							result.Author = user.Username;
							result.AuthorImageUrl = user.ProfileImageUrl;
						}

						result.ImageUrl = response?.Includes.Media.FirstOrDefault(x => x.MediaKey == result.Attachments?.MediaKeys.FirstOrDefault())?.Url
							?? string.Empty;

						return result;
					}
				}

				return null;
			}
			catch (HttpRequestException httpRequestException)
			{
				if (httpRequestException.StatusCode == HttpStatusCode.TooManyRequests)
				{
					Log.Write($"Too many requests", "Twitter");
					return null;
				}

				throw;
			}
			catch (Exception ex)
			{
				Log.Write($"Error: {ex.Message}", "Twitter");
				return null;
			}
		}

		private class Response
		{
			public required List<FashionReportEntry> Data { get; set; }
			public required ResponseIncludes Includes { get; set; }
		}

		private class ResponseIncludes
		{
			public required List<ResponseUser> Users { get; set; }
			public required List<ResponseMedia> Media { get; set; }
		}

		private class ResponseMedia
		{
			public required string MediaKey { get; set; }
			public string? Url { get; set; }
		}

		private class ResponseUser
		{
			public required string ProfileImageUrl { get; set; }
			public required string Username { get; set; }
			public required string Name { get; set; }
		}
	}
}
