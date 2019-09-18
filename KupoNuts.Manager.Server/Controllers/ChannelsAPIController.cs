// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class ChannelsAPIController : ControllerBase
	{
		[HttpGet]
		public async Task<List<Channel>> GetAsync()
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return null;

			Database<Channel> channelDb = new Database<Channel>("Channels", 1);
			await channelDb.Connect();
			List<Channel> channels = await channelDb.LoadAll();

			return channels;
		}
	}
}
