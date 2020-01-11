// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using Newtonsoft.Json;

	public class AuthenticationService : ServiceBase
	{
		[RPC]
		public string GetDiscordKey()
		{
			Settings settings = Settings.Load();
			return settings.DiscordKey;
		}

		[RPC]
		public async Task<string> AuthenticateCode(string url, string code)
		{
			Settings settings = Settings.Load();

			Dictionary<string, string> values = new Dictionary<string, string>
			{
				{ "client_id", settings.DiscordKey },
				{ "client_secret", settings.DiscordSecret },
				{ "grant_type", "authorization_code" },
				{ "code", code },
				{ "redirect_uri", url },
				{ "scope", "identify" },
			};

			HttpClient client = new HttpClient();

			// Get the user token from discord
			FormUrlEncodedContent content = new FormUrlEncodedContent(values);
			HttpResponseMessage response = await client.PostAsync("https://discordapp.com/api/oauth2/token", content);
			string responseString = await response.Content.ReadAsStringAsync();
			response.EnsureSuccessStatusCode();
			DiscordAuthResponse discordAuthResponse = JsonConvert.DeserializeObject<DiscordAuthResponse>(responseString);
			string userToken = discordAuthResponse.access_token;

			// Now get the user info from discord
			client.DefaultRequestHeaders.Add("Authorization", "Bearer " + userToken);
			response = await client.GetAsync("https://discordapp.com/api/users/@me");
			response.EnsureSuccessStatusCode();
			responseString = await response.Content.ReadAsStringAsync();
			DiscordMeResponse discordMeResponse = JsonConvert.DeserializeObject<DiscordMeResponse>(responseString);

			if (string.IsNullOrEmpty(discordMeResponse.id))
				throw new Exception("Invalid discord user Id");

			if (!this.GetIsUserAdmin(discordMeResponse.id))
				throw new Exception("User is not an administrator");

			// TODO: verify user against guilds, and return that information.

			// Finally invoke the authentication back-end
			string token = Authentication.Authenticate(discordMeResponse.id);
			return token;
		}

		// TODO: Dont use discord.net, as the bot can only access guilds she is already part of.
		// use discord web API to get all user guilds directly.
		private bool GetIsUserAdmin(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException("userId");

			foreach (SocketGuild guild in DiscordAPI.Client.Guilds)
			{
				SocketGuildUser guildUser = guild.GetUser(ulong.Parse(userId));

				if (guildUser == null)
					continue;

				foreach (SocketRole role in guildUser.Roles)
				{
					if (role.Permissions.Administrator)
					{
						return true;
					}
				}
			}

			return false;
		}

		[Serializable]
		private class DiscordAuthResponse
		{
			#pragma warning disable SA1300, SA1516
			public string access_token { get; set; }
			public string scope { get; set; }
			public string token_type { get; set; }
			public int expires_in { get; set; }
			public string refresh_token { get; set; }
			#pragma warning restore
		}

		[Serializable]
		private class DiscordMeResponse
		{
			#pragma warning disable SA1300, SA1516
			public string username { get; set; }
			public string locale { get; set; }
			public int premium_type { get; set; }
			public bool mfa_enabled { get; set; }
			public int flags { get; set; }
			public string avatar { get; set; }
			public string discriminator { get; set; }
			public string id { get; set; }
			#pragma warning restore
		}
	}
}
