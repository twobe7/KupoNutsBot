// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.IO;
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
			string content = "echo Kupo Nuts Bot Update Script";
			content += @"sleep 5s";
			content += @"git -C KupoNutsBot/ pull origin master";
			content += @"dotnet build KupoNutsBot/KupoNutsBot.sln";
			content += @"dotnet KupoNutsBot/KupoNutsBot/bin/KupoNutsBot.dll";

			File.WriteAllText("update.sh", content);

			await BashUtils.Run("chmod +x update.sh");
			await BashUtils.Run("sh update.sh", false);
			await Program.Exit();
		}
	}
}
