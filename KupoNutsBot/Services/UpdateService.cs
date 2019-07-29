// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using KupoNutsBot.Utils;

	public class UpdateService : ServiceBase
	{
		public override Task Initialize()
		{
			CommandsService.BindCommand("update", this.Update);
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("update");
			return Task.CompletedTask;
		}

		private async Task Update(string[] args, SocketMessage message)
		{
			await Program.Exit();

			await BashUtils.Run("git -C KupoNutsBot/ pull origin master");
			await BashUtils.Run("dotnet build KupoNutsBot/KupoNutsBot.sln");
			await BashUtils.Run("dotnet KupoNutsBot/KupoNutsBot/bin/KupoNutsBot.dll");
		}
	}
}
