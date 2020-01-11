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
	using FC.Manager.Server.RPC;
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
		public async Task<Client.Authentication.Data> AuthenticateCode(string url, string code)
		{
			Settings settings = Settings.Load();

			Dictionary<string, string> values = new Dictionary<string, string>
			{
				{ "client_id", settings.DiscordKey },
				{ "client_secret", settings.DiscordSecret },
				{ "grant_type", "authorization_code" },
				{ "code", code },
				{ "redirect_uri", url },
				{ "scope", Client.Authentication.DiscordScopes },
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

			// Get guilds info
			response = await client.GetAsync("https://discordapp.com/api/users/@me/guilds");
			response.EnsureSuccessStatusCode();
			responseString = await response.Content.ReadAsStringAsync();
			List<Client.Authentication.Data.Guild> guilds = JsonConvert.DeserializeObject<List<Client.Authentication.Data.Guild>>(responseString);

			List<string> canManageGuilds = new List<string>();
			foreach (Client.Authentication.Data.Guild guild in guilds)
			{
				if (!guild.CanManageGuild)
					continue;

				canManageGuilds.Add(guild.Id);
			}

			// Finally invoke the authentication back-end
			string token = Authentication.Authenticate(discordMeResponse.id, canManageGuilds);

			Client.Authentication.Data data = new Client.Authentication.Data();
			data.AuthToken = token;
			data.DiscordUserId = discordMeResponse.id;
			data.DiscordUserName = discordMeResponse.username;
			data.Guilds = guilds;

			return data;
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