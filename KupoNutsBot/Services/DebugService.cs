// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Threading.Tasks;
	using Discord.WebSocket;

	public class DebugService : ServiceBase
	{
		public override Task Initialize()
		{
			CommandsService.BindCommand("notImplemented", this.Test);
			CommandsService.BindCommand("error", this.Error);
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("notImplemented");
			CommandsService.ClearCommand("error");
			return Task.CompletedTask;
		}

		private Task Test(string[] args, SocketMessage message)
		{
			throw new NotImplementedException();
		}

		private Task Error(string[] args, SocketMessage message)
		{
			throw new Exception("This is a test error. Nothing has gone wrong.");
		}
	}
}
