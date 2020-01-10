// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Manager.Server.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Newtonsoft.Json;

	[ApiController]
	[Route("[controller]")]
	public class AuthenticationAPIController : ControllerBase
	{
		[HttpPost]
		public async Task<AuthenticationRequest> Post(AuthenticationRequest request)
		{
			try
			{
				Settings settings = Settings.Load();

				Dictionary<string, string> values = new Dictionary<string, string>
				{
					{ "client_id", settings.DiscordKey },
					{ "client_secret", settings.DiscordSecret },
					{ "grant_type", "authorization_code" },
					{ "code", request.DiscordCode },
					{ "redirect_uri", request.URL },
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

				if (string.IsNullOrEmpty(discordMeResponse.id) || !this.GetIsUserAdmin(discordMeResponse.id))
				{
					request.Message = "You must be an administrator to access this page.";
					return request;
				}

				// Finally invoke the authentication back-end
				request.Token = Authentication.Authenticate(discordMeResponse.id);
			}
			catch (Exception ex)
			{
				Log.Write(ex);
				request.Message = ex.Message;
			}

			return request;
		}

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
