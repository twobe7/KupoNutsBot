// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Items
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using Universalis;
	using XIVAPI;

	public class ItemService : ServiceBase
	{
		public static string AdvancedMeldingForbiddenEmote = @"<:AdvancedMeldingForbidden:817710837418426388> ";
		public static string ArmoireEmote = @"<:Armoire:817710861497401365> ";
		public static string ConvertToMateriaEmote = @"<:ConvertToMateria:817713212900245515> ";
		public static string CraftableEmote = @"<:craftable:817710887648493588>";
		public static string CrestEmote = @"<:CompanyCrests:817711898249920532> ";
		public static string DyeEmote = @"<:Dye:817710729917628476> ";
		public static string GilEmote = @"<:Gil:817712075609604097> ";
		public static string GlamourDresserEmote = @"<:GlamourDresser:817712475709374485> ";
		public static string HighQualityEmote = @"<:hq:817613757945217045> ";
		public static string NormalQualityEmote = @"<:nq:817613782615326732> ";
		public static string SalvageEmote = @"<:Desynth:817712274282250240> ";
		public static string UniqueEmote = @"<:Unique:817711445465628692> ";
		public static string UntradableEmote = @"<:Untradable:817710815196086283> ";

		public static IEmote HQEmote = Emote.Parse(HighQualityEmote.Trim());
		public static IEmote GilIEmote = Emote.Parse(GilEmote.Trim());

		private static readonly List<IEmote> MBEmotes = new List<IEmote>()
		{
			HQEmote, GilIEmote,
		};

		private static Task? activeMBWindowTask;

		private static Dictionary<ulong, ActiveMBWindow> activeMBEmbeds = new Dictionary<ulong, ActiveMBWindow>();

		[Command("ISearch", Permissions.Everyone, "Gets information on an item", CommandCategory.XIVData, "ItemSearch")]
		[Command("ItemSearch", Permissions.Everyone, "Gets information on an item", CommandCategory.XIVData)]
		public async Task<Embed> GetItem(ulong itemId)
		{
			Item item = await ItemAPI.Get(itemId);

			EmbedBuilder embed = item.ToEmbed();

			if (item.IsUntradable != 1)
			{
				(MarketAPI.History? hq, MarketAPI.History? nm) = await MarketAPI.GetBestPriceHistory("Elemental", itemId);

				if (hq != null | nm != null)
				{
					StringBuilder builder = new StringBuilder();
					if (hq != null)
						builder.Append(hq.ToStringEx());

					if (nm != null)
						builder.Append(nm.ToStringEx());

					embed.AddField("Best Market Board Prices", builder.ToString());
				}
			}

			return embed.Build();
		}

		[Command("ISearch", Permissions.Everyone, "Gets information on an item", CommandCategory.XIVData, "ItemSearch")]
		[Command("ItemSearch", Permissions.Everyone, "Gets information on an item", CommandCategory.XIVData)]
		public async Task<Embed> GetItem(string search)
		{
			List<SearchAPI.Result> results = await SearchAPI.Search(search, "Item");

			if (results.Count <= 0)
				throw new UserException("I couldn't find any items that match that search.");

			if (results.Count > 1)
			{
				EmbedBuilder embed = new EmbedBuilder();

				StringBuilder description = new StringBuilder();
				for (int i = 0; i < Math.Min(results.Count, 10); i++)
				{
					description.AppendLine(results[i].ID + " - " + results[i].Name);
				}

				embed.Title = results.Count + " results found";
				embed.Description = description.ToString();
				return embed.Build();
			}

			ulong? id = results[0].ID;

			if (id == null)
				throw new Exception("No Id in item");

			return await this.GetItem((ulong)id);
		}

		public async Task<Embed> GetMarketBoardItem(ulong itemId)
		{
			Item item = await ItemAPI.Get(itemId);

			EmbedBuilder embed = item.ToMbEmbed();

			if (item.IsUntradable != 1)
			{
				IOrderedEnumerable<MarketAPI.History> prices = await MarketAPI.GetBestPriceFromAllWorlds("Elemental", itemId);

				if (prices.Any())
				{
					StringBuilder hqBuilder = new StringBuilder();
					StringBuilder nqBuilder = new StringBuilder();

					if (prices.Any(x => x.Hq == true))
						hqBuilder.AppendLine("High Quality");

					if (prices.Any(x => x.Hq == false))
						nqBuilder.AppendLine("Normal Quality");

					foreach (MarketAPI.History price in prices)
					{
						if (price.Hq == true)
							hqBuilder.AppendLine(Utils.Characters.Tab + price.ToStringEx());

						if (price.Hq == false)
							nqBuilder.AppendLine(Utils.Characters.Tab + price.ToStringEx());
					}

					if (hqBuilder.Length > 0 && nqBuilder.Length > 0)
						hqBuilder.AppendLine();

					embed.AddField("Best Market Board Prices", hqBuilder.ToString() + nqBuilder.ToString());
				}
			}

			return embed.Build();
		}

		public async Task<Embed> GetMarketBoardItem(string search)
		{
			List<SearchAPI.Result> results = await SearchAPI.Search(search, "Item");

			if (results.Count <= 0)
				throw new UserException("I couldn't find any items that match that search.");

			if (results.Count > 1)
			{
				EmbedBuilder embed = new EmbedBuilder();

				StringBuilder description = new StringBuilder();
				for (int i = 0; i < Math.Min(results.Count, 10); i++)
				{
					description.AppendLine(results[i].ID + " - " + results[i].Name);
				}

				embed.Title = results.Count + " results found";
				embed.Description = description.ToString();
				return embed.Build();
			}

			ulong? id = results[0].ID;

			if (id == null)
				throw new Exception("No Id in item");

			return await this.GetMarketBoardItem((ulong)id);
		}

		[Command("MB", Permissions.Everyone, "Gets information on an item", CommandCategory.XIVData, "MarketBoard")]
		[Command("MarketBoard", Permissions.Everyone, "Gets information on an item", CommandCategory.XIVData)]
		public async Task GetMarketBoardItem(CommandMessage message, ulong itemId)
		{
			Embed embed = await this.GetMarketBoardEmbed(itemId);

			RestUserMessage mbEmbedMessage = await message.Channel.SendMessageAsync(embed: embed);

			await mbEmbedMessage.AddReactionsAsync(MBEmotes.ToArray());

			// Add to window list
			activeMBEmbeds.Add(mbEmbedMessage.Id, new ActiveMBWindow(message.Author.Id, itemId, null, false));

			// Begin the clean up task if it's not already running
			if (activeMBWindowTask == null || !activeMBWindowTask.Status.Equals(TaskStatus.Running))
			{
				Program.DiscordClient.ReactionAdded += this.OnReactionAdded;
				activeMBWindowTask = Task.Run(async () => await this.ClearReactionsAfterDelay(message.Channel));
			}
		}

		[Command("MB", Permissions.Everyone, "Gets information on an item", CommandCategory.XIVData, "MarketBoard")]
		[Command("MarketBoard", Permissions.Everyone, "Gets information on an item", CommandCategory.XIVData)]
		public async Task GetMarketBoardItem(CommandMessage message, string search)
		{
			List<SearchAPI.Result> results = await SearchAPI.Search(search, "Item");

			if (results.Count <= 0)
				throw new UserException("I couldn't find any items that match that search.");

			ulong? id;

			SearchAPI.Result exactMatch = results.FirstOrDefault(x => search.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
			if (exactMatch != null)
			{
				id = exactMatch.ID;
			}
			else if (results.Count > 1)
			{
				EmbedBuilder embed = new EmbedBuilder();

				StringBuilder description = new StringBuilder();
				for (int i = 0; i < Math.Min(results.Count, 10); i++)
				{
					description.AppendLine(results[i].ID + " - " + results[i].Name);
				}

				embed.Title = results.Count + " results found";
				embed.Description = description.ToString();

				await message.Channel.SendMessageAsync(embed: embed.Build());
				return;
			}
			else
			{
				id = results[0].ID;
			}

			if (id == null)
				throw new Exception("No Id in item");

			await this.GetMarketBoardItem(message, id.Value);
			return;
		}

		private async Task<Embed> GetMarketBoardEmbed(ulong itemId, bool? hqOnly = null, bool lowestByUnitPrice = false)
		{
			Item item = await ItemAPI.Get(itemId);

			// If item cannot be hq, ensure filter isn't hq only
			if (hqOnly == true && item.CanBeHq != 1)
				hqOnly = null;

			string tab = Utils.Characters.Space;

			EmbedBuilder embed = item.ToMbEmbed();
			embed.Description += "\n";

			if (item.IsUntradable != 1)
			{
				IOrderedEnumerable<MarketAPI.ListingDisplay> listings = await MarketAPI.GetBestPriceListing("Elemental", itemId, hqOnly, lowestByUnitPrice);

				if (listings.Any())
				{
					// Variables for world name spacing
					int longestWorldNameLength = listings.Max(x => x.WorldName?.Length ?? 0);
					int worldGap;
					string worldGapString;

					// Do thing
					StringBuilder builder = new StringBuilder();
					builder.AppendLine(Utils.Characters.Space);

					foreach (MarketAPI.ListingDisplay listing in listings)
					{
						worldGap = longestWorldNameLength - (listing.WorldName?.Length ?? 0);
						worldGapString = "{0}";

						for (int i = 0; i < worldGap; i++)
							worldGapString += "{0}";

						string line = string.Format(
							"{5} `{1} " + worldGapString + " {2} x {3}{4}`",
							tab,
							listing.WorldName,
							listing.Quantity,
							listing.MaxPricePerUnit,
							listing.Quantity == 1 ? string.Empty : " (" + listing.MaxTotal + ")",
							listing.Hq == true ? ItemService.HighQualityEmote : ItemService.NormalQualityEmote);

						builder.AppendLine(line);
					}

					builder.AppendLine();
					builder.AppendLine($"Use {ItemService.HQEmote} to toggle HQ Only.");
					builder.AppendLine($"Use {ItemService.GilIEmote} to toggle sorting by Unit Price/Total Price.");

					embed.AddField("Best Market Board Prices", builder.ToString());
				}
			}

			return embed.Build();
		}

		private async Task<Task> ClearReactionsAfterDelay(ISocketMessageChannel channel)
		{
			while (activeMBEmbeds.Count > 0)
			{
				Log.Write("MB Windows - Checking For Inactive MB Windows from total of " + activeMBEmbeds.Count, "Bot");

				DateTime runTime = DateTime.Now;

				foreach (KeyValuePair<ulong, ActiveMBWindow> mb in activeMBEmbeds)
				{
					if ((runTime - mb.Value.LastInteractedWith).TotalSeconds > 20)
					{
						Log.Write("MB Windows - Clearing MB Command: " + mb.Key, "Bot");

						IMessage message = await channel.GetMessageAsync(mb.Key);

						// Convert to UserMessage so we can edit the embed to remove the react help message
						if (message is IUserMessage userMessage)
						{
							// Get the embed and duplicate
							IEmbed embed = message.Embeds.FirstOrDefault();
							EmbedBuilder builder = new EmbedBuilder()
							.WithTitle(embed.Title)
							.WithColor(embed?.Color ?? Color.Teal)
							.WithDescription(embed?.Description)
							.WithThumbnailUrl(embed?.Thumbnail?.Url ?? string.Empty);

							// Get MB field and duplicate - remove reaction hint text
							EmbedField field = embed?.Fields.GetFirst() ?? default;
							builder.AddField(new EmbedFieldBuilder()
								.WithName(field.Name)
								.WithValue(field.Value.Substring(0, field.Value.Length - 124)));

							await userMessage.ModifyAsync(x => x.Embed = builder.Build());
						}

						// Remove reactions
						await message.RemoveAllReactionsAsync();

						activeMBEmbeds.Remove(mb.Key);
					}
				}

				Log.Write("MB Windows - Begin Wait", "Bot");
				await Task.Delay(15000);
			}

			Log.Write("MB Windows - All MB Windows Inactive", "Bot");
			Program.DiscordClient.ReactionAdded -= this.OnReactionAdded;
			return Task.CompletedTask;
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> incomingMessage, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				Log.Write("Reaction Added Market Board Command", "Bot");

				// Don't react to your own reacts!
				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return;

				// Only handle reacts to MB embed
				if (!activeMBEmbeds.ContainsKey(incomingMessage.Id))
					return;

				ActiveMBWindow mBWindow = activeMBEmbeds[incomingMessage.Id];
				mBWindow.LastInteractedWith = DateTime.Now;

				// Only handle reacts from the original user, remove the reaction
				if (mBWindow.UserId != reaction.UserId)
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					return;
				}

				// Only handle relevant reacts
				if (!MBEmotes.Contains(reaction.Emote))
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					return;
				}

				if (channel is SocketGuildChannel guildChannel)
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					bool refresh = false;

					if (reaction.Emote.Name == HQEmote.Name)
					{
						mBWindow.HqOnly = mBWindow.HqOnly == true ? (bool?)null : true;
						refresh = true;
					}
					else if (reaction.Emote.Name == GilIEmote.Name)
					{
						mBWindow.LowestByUnitPrice = !mBWindow.LowestByUnitPrice;
						refresh = true;
					}

					if (refresh)
					{
						Embed embed = await this.GetMarketBoardEmbed(mBWindow.ItemId, mBWindow.HqOnly, mBWindow.LowestByUnitPrice);
						await message.ModifyAsync(x => x.Embed = embed);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private class ActiveMBWindow
		{
			public ActiveMBWindow(ulong userId, ulong itemId, bool? hqOnly, bool lowestByUnitPrice)
			{
				this.UserId = userId;
				this.ItemId = itemId;
				this.HqOnly = hqOnly;
				this.LowestByUnitPrice = lowestByUnitPrice;

				this.LastInteractedWith = DateTime.Now;
			}

			public ulong UserId { get; set; }
			public ulong ItemId { get; set; }
			public bool? HqOnly { get; set; }
			public bool LowestByUnitPrice { get; set; }
			public DateTime LastInteractedWith { get; set; }
		}
	}
}
