// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;

	public class KarmaService : ServiceBase
	{
		public override Task Initialize()
		{
			CommandsService.BindCommand("goodbot", this.GoodBot, Permissions.Everyone, "Ye.");
			CommandsService.BindCommand("badbot", this.BadBot, Permissions.Everyone, "Na.");
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("goodbot");
			CommandsService.ClearCommand("badBot");
			return Task.CompletedTask;
		}

		private async Task GoodBot(string[] args, SocketMessage message)
		{
			Database db = Database.Load();
			db.Karma++;
			db.Save();

			await message.Channel.SendMessageAsync("Thanks!\nMy karma is " + db.Karma);
		}

		private async Task BadBot(string[] args, SocketMessage message)
		{
			Database db = Database.Load();
			db.Karma--;
			db.Save();

			await message.Channel.SendMessageAsync("Aww!\nMy karma is " + db.Karma);
		}
	}
}
