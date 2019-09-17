// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;

	public class KarmaService : ServiceBase
	{
		private const double KarmaGenerationChance = 0.1;

		private static IEmote karmaEmote = Emote.Parse("<:karma:623475895138779138>");

		private Database<Karma> karmaDatabase = new Database<Karma>("Karma", 1);

		public override async Task Initialize()
		{
			await base.Initialize();
			await this.karmaDatabase.Connect();

			Program.DiscordClient.MessageReceived += this.OnMessageReceived;
			Program.DiscordClient.ReactionAdded += this.OnReactionAdded;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.OnReactionAdded;
			Program.DiscordClient.MessageReceived -= this.OnMessageReceived;
			return base.Shutdown();
		}

		[Command("GoodBot", Permissions.Everyone, "Gives karma to the bot")]
		public async Task GoodBot(SocketMessage message)
		{
			IGuildUser fromUser = message.GetAuthor();
			IGuildUser toUser = await Program.GetBotUserForGuild(message.GetGuild());

			(Karma fromKarma, Karma toKarma) = await this.SendKarma(fromUser, toUser);

			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append("Thanks!");

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.AddField(fromUser.GetName(), fromKarma.Count);
			embedBuilder.AddField(toUser.GetName(), toKarma.Count);

			await message.Channel.SendMessageAsync(messageBuilder.ToString(), false, embedBuilder.Build());
		}

		[Command("GiveKarma", Permissions.Everyone, "Gives karma to the specified user")]
		public async Task Karma(SocketMessage message, IGuildUser destinationUser)
		{
			IGuildUser fromUser = message.GetAuthor();
			(Karma fromKarma, Karma toKarma) = await this.SendKarma(fromUser, destinationUser);

			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append("Hey ");
			messageBuilder.Append(destinationUser.Mention);
			messageBuilder.Append(", ");
			messageBuilder.Append(fromUser.Mention);
			messageBuilder.Append(" has sent you karma!");

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.AddField(fromUser.GetName(), fromKarma.Count);
			embedBuilder.AddField(destinationUser.GetName(), toKarma.Count);

			await message.Channel.SendMessageAsync(messageBuilder.ToString(), false, embedBuilder.Build());
		}

		[Command("Karma", Permissions.Everyone, "Shows your current karma")]
		public async Task<Embed> ShowKarma(SocketMessage message)
		{
			return await this.ShowKarma(message.GetAuthor());
		}

		[Command("Karma", Permissions.Everyone, "Shows the karma of the specified user")]
		public async Task<Embed> ShowKarma(IGuildUser user)
		{
			Karma fromKarma = await this.karmaDatabase.LoadOrCreate(user.Id.ToString());

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.AddField(user.GetName(), fromKarma.Count);

			return embedBuilder.Build();
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			if (message.Author.Id == Program.DiscordClient.CurrentUser.Id)
				return;

			IMessage iMessage = await message.Channel.GetMessageAsync(message.Id);

			if (iMessage is RestUserMessage restMessage)
			{
				Random rn = new Random();
				if (rn.NextDouble() < KarmaGenerationChance)
				{
					IGuildUser toUser = message.GetAuthor();
					Karma toKarma = await this.karmaDatabase.LoadOrCreate(toUser.Id.ToString());
					toKarma.Count++;
					await this.karmaDatabase.Save(toKarma);
					await restMessage.AddReactionAsync(karmaEmote);
				}
			}
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (reaction.Emote.Name != karmaEmote.Name)
				return;

			if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
				return;

			IUserMessage userMessage = await message.GetOrDownloadAsync();

			try
			{
				IGuildUser toUser = userMessage.GetAuthor();

				IGuild guild = userMessage.GetGuild();
				IGuildUser fromUser = await guild.GetUserAsync(reaction.UserId);

				await this.SendKarma(fromUser, toUser);
			}
			catch (UserException)
			{
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task<(Karma, Karma)> SendKarma(IGuildUser fromUser, IGuildUser toUser)
		{
			Karma fromKarma = await this.karmaDatabase.LoadOrCreate(fromUser.Id.ToString());

			if (fromKarma.Count <= 0)
				throw new UserException("You dont have any more karma to give!");

			Karma toKarma = await this.karmaDatabase.LoadOrCreate(toUser.Id.ToString());

			fromKarma.Count--;
			toKarma.Count++;

			await this.karmaDatabase.Save(fromKarma);
			await this.karmaDatabase.Save(toKarma);

			return (fromKarma, toKarma);
		}
	}
}
