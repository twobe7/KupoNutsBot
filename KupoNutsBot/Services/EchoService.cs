// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord.WebSocket;

	public class EchoService : ServiceBase
	{
		public override Task Initialize()
		{
			CommandsService.BindCommand("echo", this.Echo);
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			return Task.CompletedTask;
		}

		private Task Echo(string[] args, SocketMessage message)
		{
			throw new NotImplementedException();
		}
	}
}
