// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class ChannelsAPIController : ControllerBase
	{
		[HttpGet]
		public List<Channel> Get()
		{
			return Database.Load().Channels;
		}
	}
}
