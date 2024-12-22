// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Youtube
{
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using FC;
	using FC.API;
	using FC.Serialization;

	internal static class Request
	{
		private static readonly string youtubeLiveLinkText = @"<link rel=""canonical"" href=""https://www.youtube.com/watch?v=";
		private static string? key;

		private static string? Key
		{
			get
			{
				if (string.IsNullOrEmpty(key))
					key = Settings.Load().YouTubeKey;

				return key;
			}
		}

		internal static async Task<string> GetLiveVideoId(string channelId)
		{
			string videoId = null;
			string url = $"https://www.youtube.com/channel/{channelId}/live";

			try
			{
				Log.Write($"Request: {url}", "Youtube");

				using HttpClient client = new();
				using var stream = await client.GetStreamAsync(url);
				using StreamReader reader = new(stream);

				string html = await reader.ReadToEndAsync();

				int linkIndexStart = html.IndexOf(youtubeLiveLinkText);
				if (linkIndexStart != -1)
				{
					int linkIndexEnd = html.IndexOf('>', linkIndexStart + 1) - 1;
					videoId = html[(linkIndexStart + youtubeLiveLinkText.Length)..linkIndexEnd];
				}

				Log.Write($"Response: {html.Length} characters", "Youtube");

				return videoId;
			}
			catch (Exception)
			{
				throw;
			}
		}

		internal static async Task<T> LiveBroadcast<T>(string channelId)
			where T : ResponseBase
		{
			// testing
			channelId = "UCS6mQdxq09cFfAmlTaaJyFQ";

			string url = $"https://www.googleapis.com/youtube/v3/liveBroadcasts" + "&key=" + Key;

			try
			{
				////// Check token
				////if (token == null)
				////{
				////	token = await GetOAuth();
				////}
				////// TODO: this won't work, check expiry better
				////// There may also be a refresh endpoint rather than just auth again
				////else if (token.ExpiresIn < 500)
				////{
				////	token = await GetOAuth();
				////}

				Log.Write("Request: " + url, "Twitch");

				using HttpClient client = new();
				client.DefaultRequestHeaders.Add("Client-ID", Key);
				////client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.AccessToken}");

				using var stream = await client.GetStreamAsync(url);
				using StreamReader reader = new(stream);
				string json = await reader.ReadToEndAsync();

				Log.Write($"Response: {json.Length} characters", "Twitch");

				return Serializer.DeserializeResponse<T>(json, Serializer.SnakeCaseOptions);
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<T>();
				}

				throw;
			}
			catch (Exception)
			{
				////Log.Write("Error: " + ex.Message, "Twitch");
				throw;
			}
		}

		internal static async Task<T> GetYoutubeVideoInfo<T>(string videoId)
			where T : ResponseBase
		{

#if DEBUG
			// testing id
			videoId = "neokjf-dn3k";
#endif

			string url = $"https://www.googleapis.com/youtube/v3/videos?id={videoId}&part=snippet&key={Key}";

			try
			{
				Log.Write($"Request: {url}", "Youtube API");

				using HttpClient client = new();
				client.DefaultRequestHeaders.Add("Client-ID", Key);

				using StreamReader reader = new(await client.GetStreamAsync(url));
				string json = await reader.ReadToEndAsync();

				Log.Write($"Response: {json.Length} characters", "Youtube API");

				T result = Serializer.DeserializeResponse<T>(json);
				return result;
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<T>();
				}

				throw;
			}
			catch (Exception ex)
			{
				Log.Write($"Error: {ex.Message}", "Youtube API");
				throw;
			}
		}
	}
}
