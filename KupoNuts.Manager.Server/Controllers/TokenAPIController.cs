// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Net.Http;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class TokenAPIController : ControllerBase
	{
		[HttpGet]
		public string Get()
		{
			return Database.Load().Token;
		}

		[HttpPost]
		public void Post(StringData token)
		{
			Database db = Database.Load();
			db.Token = token.Value;
			db.Save();
		}
	}
}
