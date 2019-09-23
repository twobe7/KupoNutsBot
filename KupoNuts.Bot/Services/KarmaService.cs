// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;

	public class KarmaService : ServiceBase
	{
		private const double KarmaGenerationChance = 0.05;

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

		[Command("GiveKarma", Permissions.Everyone, "Gives karma to the specified user")]
		public async Task Karma(SocketMessage message, IGuildUser destinationUser)
		{
			IGuildUser fromUser = message.GetAuthor();

			Log.Write(fromUser.GetName() + " sent karma to " + destinationUser.GetName() + " (command)", "Bot");

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

		[Command("MyKarma", Permissions.Everyone, "Shows your current karma")]
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

		[Command("Karma", Permissions.Everyone, "Shows the karma leaderboards")]
		public async Task<Embed> ShowKarmaLEaders(SocketMessage message)
		{
			List<Karma> karmas = await this.karmaDatabase.LoadAll();

			karmas.Sort((Karma a, Karma b) =>
			{
				return -a.Count.CompareTo(b.Count);
			});

			int count = 10;
			if (count > karmas.Count)
				count = karmas.Count;

			IGuild guild = message.GetGuild();

			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				Karma karma = karmas[i];

				if (karma.Id == null)
					continue;

				IGuildUser user = await guild.GetUserAsync(ulong.Parse(karma.Id));
				builder.Append(karma.Count);
				builder.Append(" - ");
				builder.AppendLine(user.GetName());
			}

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Title = "Karma Leaderboard";
			embedBuilder.Description = builder.ToString();
			return embedBuilder.Build();
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			if (message.Author.Id == Program.DiscordClient.CurrentUser.Id)
				return;

			if (message.Author.IsBot)
				return;

			IMessage iMessage = await message.Channel.GetMessageAsync(message.Id);

			if (iMessage is RestUserMessage restMessage)
			{
				Random rn = new Random();
				double roll = rn.NextDouble();
				if (roll < KarmaGenerationChance)
				{
					IGuildUser toUser = message.GetAuthor();

					Log.Write(toUser.GetName() + " Generated Karma with message: \"" + message.Content + "\"", "Bot");

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

				Log.Write(toUser.GetName() + " sent karma to " + toUser.GetName() + " (reaction)", "Bot");

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

			if (toUser.IsBot)
				throw new UserException("You cant send karma to a bot!");

			Karma toKarma = await this.karmaDatabase.LoadOrCreate(toUser.Id.ToString());

			fromKarma.Count--;
			toKarma.Count++;

			await this.karmaDatabase.Save(fromKarma);
			await this.karmaDatabase.Save(toKarma);

			return (fromKarma, toKarma);
		}
	}
}
