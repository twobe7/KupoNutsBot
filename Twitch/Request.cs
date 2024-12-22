// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Twitch
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using FC;
	using FC.API;
	using FC.Serialization;

	internal static class Request
	{
		private static string? key;
		private static string? secret;
		private static OAuthToken token;

		private static string? Key
		{
			get
			{
				if (string.IsNullOrEmpty(key))
					key = Settings.Load().TwitchKey;

				return key;
			}
		}

		private static string? Secret
		{
			get
			{
				if (string.IsNullOrEmpty(secret))
					secret = Settings.Load().TwitchSecret;

				return secret;
			}
		}

		internal static async Task<T> Send<T>(string username)
			where T : ResponseBase
		{
			string url = $"https://api.twitch.tv/helix/streams?user_login={username}";

			try
			{
				// Check token
				if (token == null)
				{
					token = await GetOAuth();
				}
				// TODO: There may also be a refresh endpoint rather than just auth again
				else if (token.Expired) 
				{
					token = await GetOAuth();
				}

				Log.Write($"Request: {url}", "Twitch");

				using HttpClient client = new();
				client.DefaultRequestHeaders.Add("Client-ID", Key);
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.AccessToken}");

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
			catch (Exception ex)
			{
				Log.Write($"Error: {ex.Message}", "Twitch");
				throw;
			}
		}

		private static async Task<OAuthToken> GetOAuth()
		{
			string url = $"https://id.twitch.tv/oauth2/token";

			try
			{
				Log.Write($"Request: {url}", "Twitch");

				Dictionary<string, string> values = new()
				{
					{ "client_id", Key },
					{ "client_secret", Secret },
					{ "grant_type", "client_credentials" },
					{ "scope", string.Empty },
				};

				using HttpClient client = new();
				var response = await client.PostAsync(url, new FormUrlEncodedContent(values));

				using StreamReader reader = new (response.Content.ReadAsStream());
				string json = await reader.ReadToEndAsync();

				Log.Write($"Response: {json.Length} characters", "Twitch");

				OAuthToken result = Serializer.Deserialize<OAuthToken>(json, Serializer.SnakeCaseOptions);
				return result;
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<OAuthToken>();
				}

				throw;
			}
			catch (Exception ex)
			{
				Log.Write($"Error: {ex.Message}", "Twitch");
				throw;
			}
		}

		private static async Task<OAuthValidateToken> ValidateOAuth()
		{
			string url = "https://id.twitch.tv/oauth2/validate";

			try
			{
				Log.Write($"Request: {url}", "Twitch");

				using HttpClient client = new();
				client.DefaultRequestHeaders.Add("Authorization", $"OAuth {token.AccessToken}");

				using var stream = await client.GetStreamAsync(url);
				using StreamReader reader = new(stream);

				string json = await reader.ReadToEndAsync();

				Log.Write($"Response: {json.Length} characters", "Twitch");

				OAuthValidateToken result = Serializer.Deserialize<OAuthValidateToken>(json, Serializer.SnakeCaseOptions);
				return result;
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<OAuthValidateToken>();
				}

				throw;
			}
			catch (Exception ex)
			{
				Log.Write($"Error: {ex.Message}", "Twitch");
				throw;
			}
		}
	}
}
