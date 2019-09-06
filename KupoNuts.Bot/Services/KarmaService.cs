// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;

	public class KarmaService : ServiceBase
	{
		private Database<Karma> karmaDatabase = new Database<Karma>("Karma", 1);

		public override async Task Initialize()
		{
			await this.karmaDatabase.Connect();
		}

		[Command("GoodBot", Permissions.Everyone, "Ye.")]
		public async Task GoodBot(string[] args, SocketMessage message)
		{
			int count = await this.AddKarma(1, Program.DiscordClient.CurrentUser.Id);
			await message.Channel.SendMessageAsync("Thanks!\nMy karma is " + count);
		}

		[Command("BadBot", Permissions.Everyone, "Na.")]
		public async Task BadBot(string[] args, SocketMessage message)
		{
			if (message is SocketUserMessage userMessage)
			{
				int count = await this.AddKarma(-1, userMessage.Author.Id);
				await message.Channel.SendMessageAsync("Aww!\nYour karma is " + count);
			}
		}

		private async Task<int> AddKarma(int count, ulong userId)
		{
			Karma karma = await this.karmaDatabase.Load(userId.ToString());

			if (karma == null)
				karma = await this.karmaDatabase.CreateEntry(userId.ToString());

			karma.Count += count;

			await this.karmaDatabase.Save(karma);

			return karma.Count;
		}
	}
}
