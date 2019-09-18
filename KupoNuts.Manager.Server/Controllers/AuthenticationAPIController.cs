// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using KupoNuts.Events;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class AuthenticationAPIController : ControllerBase
	{
		[HttpPost]
		[Route("OAuth")]
		public Task Post([FromQuery] string code)
		{
			Log.Write(code, "Manager");

			return Task.CompletedTask;
			////https://discordapp.com/api/oauth2/authorize?response_type=code&client_id=604288630579527698&scope=identify&state=thisisfortesting&redirect_uri=http://ec2-13-239-119-246.ap-southeast-2.compute.amazonaws.com/AuthenticationAPI/OAuth&prompt=consent
		}
	}
}
