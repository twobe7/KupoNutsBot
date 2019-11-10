// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class ChannelsAPIController : ControllerBase
	{
		[HttpGet]
		public List<Channel> Get()
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return null;

			List<Channel> results = new List<Channel>();

			if (DiscordAPI.Client == null)
				return results;

			foreach (SocketGuild guild in DiscordAPI.Client.Guilds)
			{
				foreach (SocketGuildChannel channel in guild.Channels)
				{
					Channel.Types type = Channel.Types.Unknown;
					if (channel is SocketTextChannel)
						type = Channel.Types.Text;

					if (channel is SocketVoiceChannel)
						type = Channel.Types.Voice;

					Channel record = new Channel();
					record.DiscordId = channel.Id.ToString();
					record.Name = guild.Name + " - " + channel.Name;
					record.Type = type;
					results.Add(record);
				}
			}

			return results;
		}
	}
}
