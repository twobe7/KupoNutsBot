// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Newtonsoft.Json;

	[ApiController]
	[Route("[controller]")]
	public class AuthenticationAPIController : ControllerBase
	{
		private Database<User> userDb = new Database<User>("Users", 0);

		[HttpPost]
		public async Task<AuthenticationRequest> Post(AuthenticationRequest request)
		{
			try
			{
				await this.userDb.Connect();

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
				DiscordAuthResponse discordAuthResponse = JsonConvert.DeserializeObject<DiscordAuthResponse>(responseString);
				string userToken = discordAuthResponse.access_token;

				// Now get the user info from discord
				client.DefaultRequestHeaders.Add("Authorization", "Bearer " + userToken);
				response = await client.GetAsync("https://discordapp.com/api/users/@me");
				responseString = await response.Content.ReadAsStringAsync();
				DiscordMeResponse discordMeResponse = JsonConvert.DeserializeObject<DiscordMeResponse>(responseString);

				User user = await this.userDb.Load(discordMeResponse.id);
				if (user == null || !user.IsAdmin)
					throw new Exception("User is not an administrator");

				// Finally invoke the authentication back-end
				request.Token = Authentication.Authenticate(user.Id);
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}

			return request;
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
