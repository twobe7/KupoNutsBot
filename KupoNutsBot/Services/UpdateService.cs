// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using KupoNutsBot.Utils;

	public class UpdateService : ServiceBase
	{
		private static string updateFile = "update.sh";

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
			Log.Write("Begining Update");

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("echo Kupo Nuts Bot Update Script");
			builder.AppendLine(@"sleep 5s");
			builder.AppendLine(@"git -C KupoNutsBot/ pull origin master");
			builder.AppendLine(@"dotnet build KupoNutsBot/KupoNutsBot.sln");
			builder.AppendLine(@"dotnet KupoNutsBot/KupoNutsBot/bin/KupoNutsBot.dll");

			Log.Write("Writing update script");

			if (File.Exists(updateFile))
				File.Delete(updateFile);

			File.WriteAllText(updateFile, builder.ToString());

			Log.Write("Running update script");

			await BashUtils.Run("chmod +x " + updateFile);
			await BashUtils.Run("sh " + updateFile, false);

			Log.Write("Exiting Program");

			await Program.Exit();
		}
	}
}
