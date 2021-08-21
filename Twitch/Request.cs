// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Twitch
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
	using FC;
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
			string url = $"https://api.twitch.tv/helix/streams?user_login={username}"; // "&key=" + Key;

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

				Log.Write("Request: " + url, "Twitch");

				WebRequest req = WebRequest.Create(url);

				req.Headers.Add("Client-ID", Key);
				req.Headers.Add("Authorization", $"Bearer {token.AccessToken}");

				WebResponse response = await req.GetResponseAsync();

				using StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "Twitch");

				T result = NSSerializer.Deserialize<T>(json);
				result.Json = json;
				return result;
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<T>();
				}

				throw webEx;
			}
			catch (Exception ex)
			{
				////Log.Write("Error: " + ex.Message, "Twitch");
				throw ex;
			}
		}

		private static async Task<OAuthToken> GetOAuth()
		{
			string url = $"https://id.twitch.tv/oauth2/token?client_id={Key}&client_secret={Secret}&grant_type=client_credentials&scope=";

			try
			{
				Log.Write("Request: " + url, "Twitch");

				WebRequest req = WebRequest.Create(url);
				req.Method = "POST";

				WebResponse response = await req.GetResponseAsync();

				using StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "Twitch");

				OAuthToken result = NSSerializer.Deserialize<OAuthToken>(json);
				return result;
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<OAuthToken>();
				}

				throw webEx;
			}
			catch (Exception ex)
			{
				////Log.Write("Error: " + ex.Message, "Twitch");
				throw ex;
			}
		}

		private static async Task<OAuthValidateToken> ValidateOAuth()
		{
			string url = $"https://id.twitch.tv/oauth2/validate";

			try
			{
				Log.Write("Request: " + url, "Twitch");

				WebRequest req = WebRequest.Create(url);
				req.Headers.Add("Authorization", $"OAuth {token.AccessToken}");

				WebResponse response = await req.GetResponseAsync();

				using StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "Twitch");

				OAuthValidateToken result = NSSerializer.Deserialize<OAuthValidateToken>(json);
				return result;
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<OAuthValidateToken>();
				}

				throw webEx;
			}
			catch (Exception ex)
			{
				////Log.Write("Error: " + ex.Message, "Twitch");
				throw ex;
			}
		}
	}
}
