// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class AuthenticationAPIController : ControllerBase
	{
		[HttpPost]
		public async Task<AuthenticationRequest> Post(AuthenticationRequest request)
		{
			Settings settings = Settings.Load();

			string url = this.Request.PathBase;

			Dictionary<string, string> values = new Dictionary<string, string>
			{
				{ "client_id", settings.DiscordKey },
				{ "client_secret", settings.DiscordSecret },
				{ "grant_type", "authorization_code" },
				{ "code", request.DiscordCode },
				{ "redirect_uri", url },
				{ "scope", "identify" },
			};

			HttpClient client = new HttpClient();
			FormUrlEncodedContent content = new FormUrlEncodedContent(values);
			HttpResponseMessage response = await client.PostAsync("https://discordapp.com/api/oauth2/token", content);
			string responseString = await response.Content.ReadAsStringAsync();

			Log.Write(">> " + responseString, "Auth");

			request.Token = Authentication.Authenticate(request.DiscordCode);

			return request;
		}
	}
}
