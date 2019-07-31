// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using KupoNutsBot.Commands;
	using KupoNutsBot.Utils;

	public class UpdateService : ServiceBase
	{
		private static string updateFile = "update.sh";

		public override async Task Initialize()
		{
			CommandsService.BindCommand("update", this.Update, Permissions.Administrators, "Updates the bot from GitHUB source and reboots.");

			if (Database.Instance.StatusChannel != 0)
			{
				SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(Database.Instance.StatusChannel);

				if (channel != null)
				{
					await channel.SendMessageAsync("I'm back! Updated successfully.");

					Database.Instance.StatusChannel = 0;
					Database.Instance.Save();
				}
			}
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("update");
			return Task.CompletedTask;
		}

		private async Task Update(string[] args, SocketMessage message)
		{
			Database.Instance.StatusChannel = message.Channel.Id;
			Database.Instance.Save();
			await message.Channel.SendMessageAsync("I'll be right back!");
			Log.Write("Begining Update");

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("echo Kupo Nuts Bot Update Script");
			builder.AppendLine(@"sleep 5s");
			builder.AppendLine(@"git -C KupoNutsBot/ pull origin master");
			builder.AppendLine(@"dotnet build KupoNutsBot/KupoNutsBot.sln");
			builder.AppendLine(@"dotnet KupoNutsBot/bin/KupoNutsBot.dll");

			Log.Write("Writing update script");

			if (File.Exists(updateFile))
				File.Delete(updateFile);

			File.WriteAllText(updateFile, builder.ToString());

			Log.Write("Running update script");

			await CommandLine.RunAsync("chmod +x " + updateFile);
			CommandLine.Run("sh " + updateFile);

			Log.Write("Exiting Program");

			Program.Exit();
		}
	}
}
