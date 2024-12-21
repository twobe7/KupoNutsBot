// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Quotes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Data;
	using FC.Quotes;
	using static FC.Events.Event;

	[Group("quotes", "Record the words of others")]
	public class QuoteService : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient;

		private static readonly Table<Quote> QuoteDb = new("KupoNuts_Quotes", Quote.Version);

		public QuoteService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		public override async Task Initialize()
		{
			await base.Initialize();
			await QuoteDb.Connect();

			this.DiscordClient.ReactionAdded += this.OnReactionAdded;
		}

		public override Task Shutdown()
		{
			this.DiscordClient.ReactionAdded -= this.OnReactionAdded;
			return base.Shutdown();
		}

		[SlashCommand("get", "Gets a quote")]
		public async Task GetQuote(
			[Summary("user", "Get quotes from a specific user")]
			SocketGuildUser? user = null,
			[Summary("id", "Get a specific quote by id")]
			int? quoteId = null)
		{
			await this.DeferAsync();

			// Base query
			var query = new Dictionary<string, object>
			{
				{ "GuildId", this.Context.Guild.Id },
			};

			// Add user if it was specified
			if (user != null)
				query.Add("UserId", user.Id);

			if (quoteId != null)
				query.Add("QuoteId", quoteId);

			// Get all quotes for current Guild
			List<Quote> quotes = await QuoteDb.LoadAll(query);

			if (quotes.Count <= 0)
			{
				string errMessage = "There are no quotes yet! Try reacting to a message with a 💬!";
				if (user != null)
				{
					errMessage = "There are no quotes by that user yet!";
				}
				else if (quoteId != null)
				{
					errMessage = "I couldn't find a quote with that id!";
				}

				await this.FollowupAsync(errMessage);
			}

			int index = new Random().Next(quotes.Count);

			await this.FollowupAsync(embeds: new Embed[] { this.GetEmbed(quotes[index]) });
		}

		[SlashCommand("list", "Lists all quotes")]
		public async Task GetQuotes(
			[Summary("user", "Get quotes from a specific user")]
			SocketGuildUser? user = null,
			int? page = null)
		{
			await this.DeferAsync();

			// Base query
			var query = new Dictionary<string, object>
			{
				{ "GuildId", this.Context.Guild.Id },
			};

			// Add user if it was specified
			if (user != null)
				query.Add("UserId", user.Id);

			// Get all quotes for current Guild
			List<Quote> quotes = await QuoteDb.LoadAll(query);

			if (quotes.Count <= 0)
			{
				await this.FollowupAsync("There are no quotes that match this search! Try reacting to a message with a 💬!");
				return;
			}

			quotes.Sort((x, y) =>
			{
				return x.QuoteId.CompareTo(y.QuoteId);
			});

			int numPages = (int)Math.Ceiling((double)quotes.Count / 20.0);

			// start pages at 1, so there is no page 0.
			var selectedPage = Math.Max(page ?? 1, 1) - 1;
			int min = 20 * selectedPage;
			int max = Math.Min(quotes.Count, 20 * (selectedPage + 1));

			// Adjust min where total is less than min
			if (quotes.Count < min)
				min = 0;

			StringBuilder quotesList = new();
			for (int i = min; i < max; i++)
			{
				Quote quote = quotes[i];
				quotesList.Append($"{quote.QuoteId} - ");
				quotesList.AppendLine(quote.Content.RemoveLineBreaks().Truncate(30));
			}

			if (numPages > 1)
			{
				quotesList.AppendLine();
				quotesList.Append($"Page {selectedPage + 1} of {numPages}");
			}

			EmbedBuilder builder = new EmbedBuilder()
				.WithDescription(quotesList.ToString());

			if (user != null)
			{
				builder.Author = new EmbedAuthorBuilder()
					.WithName(user.GetName())
					.WithIconUrl(user.GetAvatarUrl());
			}

			await this.FollowupAsync(embeds: new Embed[] { builder.Build() });
		}

		[SlashCommand("delete", "Deletes a quote from the given user")]
		public async Task DeleteQuote(
			[Summary("id", "The quote Id to be deleted")]
			int quoteId)
		{
			await this.DeferAsync();

			Dictionary<string, object> filters = new()
			{
				{ "GuildId", this.Context.Guild.Id },
				{ "QuoteId", quoteId },
			};

			Quote? quote = (await QuoteDb.LoadAll(filters)).FirstOrDefault();

			if (quote == null)
			{
				await this.FollowupAsync("Unable to find quote by that Id!");
				return;
			}

			if (quote.UserId != this.Context.User.Id
				&& this.Context.User is IGuildUser guildUser
				&& CommandsService.GetPermissions(guildUser) != Permissions.Administrators)
			{
				await this.FollowupAsync("You don't have permission to do that!");
				return;
			}

			await QuoteDb.Delete(quote);

			await this.FollowupAsync("Quote deleted!");
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageCache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			try
			{
				if (reaction.Emote.Name != "💬")
					return;

				if (channel is Cacheable<IMessageChannel, ulong> messageChannel
					&& reaction.Channel is IGuildChannel guildChannel)
				{
					IUserMessage message = await messageCache.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					if (string.IsNullOrEmpty(message.Content))
						return;

					Quote quote = await QuoteDb.LoadOrCreate($"{message.Author.Id}_{message.Id}");
					quote.Content = message.Content;
					quote.UserId = message.Author.Id;
					quote.GuildId = guildChannel.GuildId;
					quote.MessageLink = this.GetMessageLink(message);
					quote.UserName = message.Author.Username;
					quote.QuoteId = await this.GetNextQuoteId(message.GetGuild(), message.GetAuthor());
					quote.SetDateTime(message.CreatedAt);
					await QuoteDb.Save(quote);

					Log.Write("Got quote: " + message.Content, "Bot");
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task<int> GetNextQuoteId(IGuild guild, IUser user)
		 => await this.GetNextQuoteId(guild.Id, user.Id);

		private async Task<int> GetNextQuoteId(ulong guildId, ulong userId)
		{
			Dictionary<string, object> filters = new()
			{
				{ "UserId", userId },
				{ "GuildId", guildId },
			};

			HashSet<int> idList = (await QuoteDb.LoadAll(filters))
				.Select(x => x.QuoteId)
				.ToHashSet();

			return idList.Count == 0 ? 1 : idList.Max() + 1;
		}

		private Embed GetEmbed(Quote self)
		{
			SocketGuild guild = this.DiscordClient.GetGuild(self.GuildId);
			SocketGuildUser user = guild.GetUser(self.UserId);

			EmbedBuilder builder = new()
			{
				Author = new EmbedAuthorBuilder
				{
					Name = user.GetName(),
					IconUrl = user.GetAvatarUrl(),
				},
				Description = self.GetQuoteDescription(),
				Timestamp = self.GetDateTime().ToDateTimeOffset(),

				Footer = new EmbedFooterBuilder
				{
					Text = $"Id: {self.QuoteId}",
				},
			};

			return builder.Build();
		}

		private string GetMessageLink(IUserMessage message)
			=> $"https://discord.com/channels/{message.GetGuild().Id}/{message.Channel.Id}/{message.Id}";
	}
}
