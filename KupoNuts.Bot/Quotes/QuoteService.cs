// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Quotes
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using KupoNuts.Quotes;
	using NodaTime;

	public class QuoteService : ServiceBase
	{
		private Database<Quote> quoteDb = new Database<Quote>("Quotes", Quote.Version);

		public override async Task Initialize()
		{
			await this.quoteDb.Connect();

			Program.DiscordClient.ReactionAdded += this.OnReactionAdded;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.OnReactionAdded;
			return base.Shutdown();
		}

		[Command("Quote", Permissions.Everyone, "Gets a random quote")]
		public async Task<Embed> GetQuote()
		{
			List<Quote> quotes = await this.quoteDb.LoadAll();

			if (quotes.Count <= 0)
				throw new UserException("There are no quotes yet! Try reacting to a message with a 💬!");

			Random rn = new Random();
			int index = rn.Next(quotes.Count);

			return quotes[index].ToEmbed();
		}

		[Command("Quote", Permissions.Everyone, "Gets a random quote from a user")]
		public async Task<Embed> GetQuote(IUser user)
		{
			List<Quote> allQuotes = await this.quoteDb.LoadAll();

			List<Quote> quotes = new List<Quote>();
			foreach (Quote quote in allQuotes)
			{
				if (quote.UserId != user.Id)
					continue;

				quotes.Add(quote);
			}

			if (quotes.Count <= 0)
				throw new UserException("There are no quotes from that user yet! Try reacting to a message with a 💬!");

			Random rn = new Random();
			int index = rn.Next(quotes.Count);

			return quotes[index].ToEmbed();
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageCache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				if (reaction.Emote.Name != "💬")
					return;

				if (channel is SocketGuildChannel guildChannel)
				{
					IUserMessage message = await messageCache.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					if (string.IsNullOrEmpty(message.Content))
						return;

					Quote quote = await this.quoteDb.LoadOrCreate(message.Author.Id + "_" + message.Id);
					quote.Content = message.Content;
					quote.UserId = message.Author.Id;
					quote.GuildId = guildChannel.Guild.Id;
					quote.UserName = message.Author.Username;
					quote.SetDateTime(message.CreatedAt);
					await this.quoteDb.Save(quote);

					Log.Write("Got quote: " + message.Content, "Bot");
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
