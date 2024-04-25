// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web.Services;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FC.Manager.Web.RPC;
using FC.Serialization;

public class AuthenticationService : ServiceBase
{
	[RPC]
	public string GetDiscordKey()
	{
		Settings settings = Settings.Load();

		if (string.IsNullOrEmpty(settings.DiscordKey))
			throw new Exception("No Discord Key in settings file");

		return settings.DiscordKey;
	}

	[RPC]
	public async Task<Authentication.Data> AuthenticateCode(string url, string code)
	{
		Settings settings = Settings.Load();

		Dictionary<string, string> values = new ()
		{
			{ "client_id", settings.DiscordKey },
			{ "client_secret", settings.DiscordSecret },
			{ "grant_type", "authorization_code" },
			{ "code", code },
			{ "redirect_uri", url },
			{ "scope", Authentication.DiscordScopes },
		};

		HttpClient client = new ();

		// Get the user token from discord
		FormUrlEncodedContent content = new (values);
		HttpResponseMessage response = await client.PostAsync("https://discordapp.com/api/oauth2/token", content);
		string responseString = await response.Content.ReadAsStringAsync();
		response.EnsureSuccessStatusCode();
		DiscordAuthResponse discordAuthResponse = Serializer.Deserialize<DiscordAuthResponse>(responseString);
		string userToken = discordAuthResponse.access_token;

		// Now get the user info from discord
		client.DefaultRequestHeaders.Add("Authorization", "Bearer " + userToken);
		response = await client.GetAsync("https://discordapp.com/api/users/@me");
		response.EnsureSuccessStatusCode();
		responseString = await response.Content.ReadAsStringAsync();
		DiscordMeResponse discordMeResponse = Serializer.Deserialize<DiscordMeResponse>(responseString);

		if (string.IsNullOrEmpty(discordMeResponse.id))
			throw new Exception("Invalid discord user Id");

		// Get guilds info
		response = await client.GetAsync("https://discordapp.com/api/users/@me/guilds");
		response.EnsureSuccessStatusCode();
		responseString = await response.Content.ReadAsStringAsync();
		List<Authentication.Data.Guild> guilds = Serializer.Deserialize<List<Authentication.Data.Guild>>(responseString);

		List<ulong> canManageGuilds = [];
		foreach (Authentication.Data.Guild guild in guilds)
		{
			// Check if already connected to Guild
			guild.IsInGuild = DiscordService.DiscordClient.GetGuild(guild.GetId()) != null;

			if (!guild.CanManageGuild)
				continue;

			canManageGuilds.Add(guild.GetId());
		}

		// Finally invoke the authentication back-end
		string token = Authentication.Authenticate(discordMeResponse.id, canManageGuilds);

		Authentication.Data data = new ()
		{
			AuthToken = token,
			DiscordUserId = discordMeResponse.id,
			DiscordUserName = discordMeResponse.username,
			Guilds = guilds
		};

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