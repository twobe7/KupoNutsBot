// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Quotes
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Data;
	using FC.Quotes;
	using NodaTime;

	public class QuoteService : ServiceBase
	{
		private Table<Quote> quoteDb = new Table<Quote>("KupoNuts_Quotes", Quote.Version);

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
			return await this.GetQuotes(message, user, 1);
		}

		[Command("Quotes", Permissions.Everyone, "Lists all quotes for the given user")]
		public async Task<Embed> GetQuotes(CommandMessage message, IUser user, int page)
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

			int numPages = (int)Math.Ceiling((double)quotes.Count / 20.0);

			// start pages at 1, so there is no page 0.
			page = Math.Max(page, 1) - 1;
			int min = 20 * page;
			int max = Math.Min(quotes.Count, 20 * (page + 1));

			StringBuilder quotesList = new StringBuilder();
			for (int i = min; i < max; i++)
			{
				Quote quote = quotes[i];
				quotesList.Append(quote.QuoteId);
				quotesList.Append(" - ");
				quotesList.AppendLine(quote.Content.RemoveLineBreaks().Truncate(30));
			}

			if (numPages > 1)
			{
				quotesList.AppendLine();
				quotesList.Append("Page ");
				quotesList.Append(page + 1);
				quotesList.Append(" of ");
				quotesList.Append(numPages);
			}

			EmbedBuilder builder = new EmbedBuilder();
			builder.Author = new EmbedAuthorBuilder();
			builder.Author.Name = guildUser.GetName();
			builder.Author.IconUrl = guildUser.GetAvatarUrl();
			builder.Description = quotesList.ToString();
			return builder.Build();
		}

		[Command("DeleteQuote", Permissions.Everyone, "Deletes a quote from the given user")]
		public async Task<string> DeleteQuote(CommandMessage message, IUser user, int quoteId)
		{
			if (message.Author != user && CommandsService.GetPermissions(message.Author) != Permissions.Administrators)
				throw new UserException("You don't have permission to do that.");

			Dictionary<string, object> filters = new Dictionary<string, object>();
			filters.Add("UserId", user.Id);
			filters.Add("GuildId", message.Guild.Id);
			filters.Add("QuoteId", quoteId);

			List<Quote> allQuotes = await this.quoteDb.LoadAll(filters);

			if (allQuotes.Count <= 0)
				throw new UserException("I couldn't find that quote from that user.");

			foreach (Quote quote in allQuotes)
			{
				await this.quoteDb.Delete(quote);
			}

			return "Quote deleted!";
		}

		[Command("DeleteQuote", Permissions.Everyone, "Deletes a quote from yourself")]
		public Task<string> DeleteQuote(CommandMessage message, int quoteId)
		{
			return this.DeleteQuote(message, message.Author, quoteId);
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
					quote.QuoteId = await this.GetNextQuoteId(message.GetGuild(), message.GetAuthor());
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

		private async Task<int> GetNextQuoteId(IGuild guild, IUser user)
		{
			return await this.GetNextQuoteId(guild.Id, user.Id);
		}

		private async Task<int> GetNextQuoteId(ulong guildId, ulong userId)
		{
			Dictionary<string, object> filters = new Dictionary<string, object>();
			filters.Add("UserId", userId);
			filters.Add("GuildId", guildId);

			List<Quote> allQuotes = await this.quoteDb.LoadAll(filters);

			HashSet<int> allIds = new HashSet<int>();
			foreach (Quote quote in allQuotes)
			{
				allIds.Add(quote.QuoteId);
			}

			int index = 1;
			while (true)
			{
				if (!allIds.Contains(index))
					return index;

				index++;
			}
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
