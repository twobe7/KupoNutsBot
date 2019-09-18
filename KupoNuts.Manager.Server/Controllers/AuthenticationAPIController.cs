// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class AuthenticationAPIController : ControllerBase
	{
		[HttpPost]
		public Task<AuthenticationRequest> Post(AuthenticationRequest request)
		{
			request.Token = Authentication.Authenticate(request.DiscordCode);

			return Task.FromResult(request);
		}
	}
}
