// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Quotes
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using KupoNuts.Data;
	using KupoNuts.Quotes;
	using NodaTime;

	public class QuoteService : ServiceBase
	{
		private Table<Quote> quoteDb = Table<Quote>.Create("Quotes", Quote.Version);

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
		public async Task<Embed> GetQuote(CommandMessage message)
		{
			List<Quote> allQuotes = await this.quoteDb.LoadAll();

			List<Quote> quotes = new List<Quote>();
			foreach (Quote quote in allQuotes)
			{
				if (quote.GuildId != message.Guild.Id)
					continue;

				quotes.Add(quote);
			}

			if (quotes.Count <= 0)
				throw new UserException("There are no quotes yet! Try reacting to a message with a 💬!");

			Random rn = new Random();
			int index = rn.Next(quotes.Count);

			return this.GetEmbed(quotes[index]);
		}

		[Command("Quote", Permissions.Everyone, "Gets a quote from yourself")]
		public async Task<Embed> GetQuote(CommandMessage message, int id)
		{
			return await this.GetQuote(message, message.Author, id);
		}

		[Command("Quote", Permissions.Everyone, "Gets a random quote from a user")]
		public async Task<Embed> GetQuote(CommandMessage message, IUser user)
		{
			List<Quote> allQuotes = await this.quoteDb.LoadAll();

			List<Quote> quotes = new List<Quote>();
			foreach (Quote quote in allQuotes)
			{
				if (quote.GuildId != message.Guild.Id)
					continue;

				if (quote.UserId != user.Id)
					continue;

				quotes.Add(quote);
			}

			if (quotes.Count <= 0)
				throw new UserException("There are no quotes from that user yet! Try reacting to a message with a 💬!");

			Random rn = new Random();
			int index = rn.Next(quotes.Count);

			return this.GetEmbed(quotes[index]);
		}

		[Command("Quote", Permissions.Everyone, "Gets a quote from a user")]
		public async Task<Embed> GetQuote(CommandMessage message, IUser user, int id)
		{
			List<Quote> allQuotes = await this.quoteDb.LoadAll();

			foreach (Quote quote in allQuotes)
			{
				if (quote.GuildId != message.Guild.Id)
					continue;

				if (quote.UserId != user.Id)
					continue;

				if (quote.QuoteId != id)
					continue;

				return this.GetEmbed(quote);
			}

			throw new UserException("I couldn't find a quote with that id.");
		}

		[Command("Quotes", Permissions.Everyone, "Lists all quotes from yourself")]
		public async Task<Embed> GetQuotes(CommandMessage message)
		{
			return await this.GetQuotes(message, message.Author);
		}

		[Command("Quotes", Permissions.Everyone, "Lists all quotes for the given user")]
		public async Task<Embed> GetQuotes(CommandMessage message, IUser user)
		{
			List<Quote> allQuotes = await this.quoteDb.LoadAll();

			List<Quote> quotes = new List<Quote>();
			foreach (Quote quote in allQuotes)
			{
				if (quote.UserId != user.Id)
					continue;

				if (quote.GuildId != message.Guild.Id)
					continue;

				quotes.Add(quote);
			}

			if (quotes.Count <= 0)
				throw new UserException("There are no quotes from that user yet! Try reacting to a message with a 💬!");

			quotes.Sort((x, y) =>
			{
				return x.QuoteId.CompareTo(y.QuoteId);
			});

			IGuildUser guildUser = await message.Guild.GetUserAsync(user.Id);

			StringBuilder quotesList = new StringBuilder();
			foreach (Quote quote in quotes)
			{
				quotesList.Append(quote.QuoteId);
				quotesList.Append(" - ");
				quotesList.AppendLine(quote.Content.RemoveLineBreaks().Truncate(30));
			}

			EmbedBuilder builder = new EmbedBuilder();
			builder.Author = new EmbedAuthorBuilder();
			builder.Author.Name = guildUser.GetName();
			builder.Author.IconUrl = guildUser.GetAvatarUrl();
			builder.Description = quotesList.ToString();
			return builder.Build();
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
					quote.QuoteId = await this.GetNextQuoteId(message.GetAuthor());
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

		private async Task<int> GetNextQuoteId(IUser user)
		{
			return await this.GetNextQuoteId(user.Id);
		}

		private async Task<int> GetNextQuoteId(ulong userId)
		{
			List<Quote> allQuotes = await this.quoteDb.LoadAll();

			int index = 0;
			foreach (Quote quote in allQuotes)
			{
				if (quote.UserId != userId)
					continue;

				if (quote.QuoteId >= index)
				{
					index = quote.QuoteId + 1;
				}
			}

			return index;
		}

		private Embed GetEmbed(Quote self)
		{
			SocketGuild guild = Program.DiscordClient.GetGuild((ulong)self.GuildId);
			SocketGuildUser user = guild.GetUser((ulong)self.UserId);

			EmbedBuilder builder = new EmbedBuilder();

			builder.Author = new EmbedAuthorBuilder();
			builder.Author.Name = user.GetName();
			builder.Author.IconUrl = user.GetAvatarUrl();
			builder.Description = self.Content;
			builder.Timestamp = self.GetDateTime().ToDateTimeOffset();

			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = "Id: " + self.QuoteId;

			return builder.Build();
		}
	}
}
