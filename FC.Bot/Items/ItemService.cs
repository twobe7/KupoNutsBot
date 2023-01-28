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
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Services;
	using FC.XIVData;
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

		public readonly DiscordSocketClient DiscordClient;

		private static readonly List<IEmote> MBEmotes = new List<IEmote>()
		{
			HQEmote, GilIEmote,
		};

		private static Task? activeMBWindowTask;

		private static Dictionary<ulong, ActiveMBWindow> activeMBEmbeds = new Dictionary<ulong, ActiveMBWindow>();

		public ItemService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		[SlashCommand("item-search", "Gets information on an item")]
		public async Task GetItem([Autocomplete(typeof(ItemAutocompleteHandler))] string search)
		{
			await this.DeferAsync();

			if (ulong.TryParse(search, out ulong searchAsUlong))
			{
				await this.FollowupAsync(embed: await this.GetItem(searchAsUlong));
				return;
			}

			List<SearchAPI.Result> results = await SearchAPI.Search(search, "Item");

			if (results.Count <= 0)
				throw new UserException("I couldn't find any items that match that search.");

			if (results.Count > 1)
			{
				EmbedBuilder embed = new ();

				StringBuilder description = new ();
				for (int i = 0; i < Math.Min(results.Count, 10); i++)
				{
					description.AppendLine(results[i].ID + " - " + results[i].Name);
				}

				embed.Title = $"{results.Count} results found for \"{search}\"";
				embed.Description = description.ToString();

				await this.FollowupAsync(embed: embed.Build());
				return;
			}

			ulong? id = results[0].ID;

			if (id == null)
				throw new Exception("No Id in item");

			await this.FollowupAsync(embed: await this.GetItem((ulong)id));
		}

		[SlashCommand("marketboard", "Retrieves item listing from Universalis Marketboard")]
		public async Task GetMarketBoardItem(DataCentre dataCentre, [Autocomplete(typeof(ItemAutocompleteHandler))] string search)
		{
			await this.DeferAsync();

			if (ulong.TryParse(search, out ulong searchAsUlong))
			{
				await this.GetMarketBoardItem(dataCentre, searchAsUlong);
				return;
			}

			List<SearchAPI.Result> results = await SearchAPI.Search(search, "Item");

			if (results.Count <= 0)
				throw new UserException("I couldn't find any items that match that search.");

			ulong? id;

			SearchAPI.Result? exactMatch = results.FirstOrDefault(x => search.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
			if (exactMatch != null)
			{
				id = exactMatch.ID;
			}
			else if (results.Count > 1)
			{
				EmbedBuilder embed = new ();

				StringBuilder description = new ();
				for (int i = 0; i < Math.Min(results.Count, 10); i++)
				{
					description.AppendLine(results[i].ID + " - " + results[i].Name);
				}

				embed.Title = $"{results.Count} results found for \"{search}\"";
				embed.Description = description.ToString();

				embed.AddField("Data Centre", dataCentre.ToDisplayString());

				await this.FollowupAsync(embed: embed.Build(), ephemeral: true);
				return;
			}
			else
			{
				id = results[0].ID;
			}

			if (id == null)
				throw new Exception("No Id in item");

			await this.GetMarketBoardItem(dataCentre, id.Value);
		}

		private async Task<Embed> GetItem(ulong itemId)
		{
			Item item = await ItemAPI.Get(itemId);

			EmbedBuilder embed = item.ToEmbed();

			if (item.IsUntradable != 1)
			{
				// TODO: Add default data centre here
				(MarketAPI.History? hq, MarketAPI.History? nm) = await MarketAPI.GetBestPriceHistory("Materia", itemId);

				if (hq != null | nm != null)
				{
					StringBuilder builder = new ();
					if (hq != null)
						builder.Append(hq.ToStringEx());

					if (nm != null)
						builder.Append(nm.ToStringEx());

					embed.AddField("Best Market Board Prices", builder.ToString());
				}
			}

			return embed.Build();
		}

		private async Task GetMarketBoardItem(DataCentre dataCentre, ulong itemId)
		{
			Embed embed = await this.GetMarketBoardEmbed(itemId, dataCentre);

			var mbEmbedMessage = await this.FollowupAsync(embed: embed);

			await mbEmbedMessage.AddReactionsAsync(MBEmotes.ToArray());

			// Add to window list
			activeMBEmbeds.Add(mbEmbedMessage.Id, new ActiveMBWindow(this.Context.User.Id, itemId, dataCentre, null, false));

			// Begin the clean up task if it's not already running
			if (activeMBWindowTask == null || !activeMBWindowTask.Status.Equals(TaskStatus.Running))
			{
				this.DiscordClient.ReactionAdded += this.OnReactionAdded;
				activeMBWindowTask = Task.Run(async () => await this.ClearReactionsAfterDelay(this.Context.Channel));
			}
		}

		////private async Task<Embed> GetMarketBoardItem(ulong itemId)
		////{
		////	Item item = await ItemAPI.Get(itemId);

		////	EmbedBuilder embed = item.ToMbEmbed();

		////	if (item.IsUntradable != 1)
		////	{
		////		IOrderedEnumerable<MarketAPI.History> prices = await MarketAPI.GetBestPriceFromAllWorlds("Materia", itemId);

		////		if (prices.Any())
		////		{
		////			StringBuilder hqBuilder = new ();
		////			StringBuilder nqBuilder = new ();

		////			if (prices.Any(x => x.Hq == true))
		////				hqBuilder.AppendLine("High Quality");

		////			if (prices.Any(x => x.Hq == false))
		////				nqBuilder.AppendLine("Normal Quality");

		////			foreach (MarketAPI.History price in prices)
		////			{
		////				if (price.Hq == true)
		////					hqBuilder.AppendLine(Utils.Characters.Tab + price.ToStringEx());

		////				if (price.Hq == false)
		////					nqBuilder.AppendLine(Utils.Characters.Tab + price.ToStringEx());
		////			}

		////			if (hqBuilder.Length > 0 && nqBuilder.Length > 0)
		////				hqBuilder.AppendLine();

		////			embed.AddField("Best Market Board Prices", hqBuilder.ToString() + nqBuilder.ToString());
		////		}
		////	}

		////	return embed.Build();
		////}

		private async Task<Embed> GetMarketBoardEmbed(ulong itemId, DataCentre dataCentre, bool? hqOnly = null, bool lowestByUnitPrice = false)
		{
			bool canBeHq, isUntradeable;
			EmbedBuilder embed;

			if (Items.XivItemsById.TryGetValue(Convert.ToInt32(itemId), out XivItem? xivItem))
			{
				canBeHq = xivItem.CanBeHq;
				isUntradeable = xivItem.IsUntradable;
				embed = xivItem.ToMbEmbed();
			}
			else
			{
				Item item = await ItemAPI.Get(itemId);
				canBeHq = item.CanBeHq == 1;
				isUntradeable = item.IsUntradable == 1;
				embed = item.ToMbEmbed();
			}

			// If item cannot be hq, ensure filter isn't hq only
			if (hqOnly == true && !canBeHq)
				hqOnly = null;

			embed.Description += "\n";

			embed.AddField("Data Centre", dataCentre.ToDisplayString());

			if (!isUntradeable)
			{
				IOrderedEnumerable<MarketAPI.ListingDisplay> listings = await MarketAPI.GetBestPriceListing(dataCentre.ToString(), itemId, hqOnly, lowestByUnitPrice);

				if (listings.Any())
				{
					// Variables for world name spacing
					int longestWorldNameLength = listings.Max(x => x.WorldName?.Length ?? 0);
					int worldGap;
					string worldGapString;

					// Start building pricing list
					StringBuilder builder = new ();
					builder.AppendLine(Utils.Characters.Space);

					foreach (MarketAPI.ListingDisplay listing in listings)
					{
						worldGap = longestWorldNameLength - (listing.WorldName?.Length ?? 0);
						worldGapString = "{0}";

						for (int i = 0; i < worldGap; i++)
							worldGapString += "{0}";

						string line = string.Format(
							"{6} {5} `{1} " + worldGapString + " {2} x {3}{4}`",
							Utils.Characters.Space,
							listing.WorldName,
							listing.Quantity,
							listing.MaxPricePerUnit,
							listing.Quantity == 1 ? string.Empty : " (" + listing.MaxTotal + ")",
							listing.Hq == true ? HighQualityEmote : NormalQualityEmote,
							listing.LastUpdatedIcon);

						builder.AppendLine(line);
					}

					builder.AppendLine();
					builder.AppendLine($"Use {HQEmote} to toggle HQ Only.");
					builder.AppendLine($"Use {GilIEmote} to toggle sorting by Unit Price/Total Price.");

					embed.AddField("Best Market Board Prices", builder.ToString());
				}
			}

			return embed.Build();
		}

		private async Task<Task> ClearReactionsAfterDelay(IMessageChannel channel)
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
							IEmbed? embed = message.Embeds.FirstOrDefault();
							EmbedBuilder builder = new EmbedBuilder()
								.WithTitle(embed?.Title)
								.WithColor(embed?.Color ?? Color.Teal)
								.WithDescription(embed?.Description)
								.WithThumbnailUrl(embed?.Thumbnail?.Url ?? string.Empty);

							// Duplicate fields on embed
							var embedFields = embed?.Fields;
							if (embedFields != null)
							{
								foreach (var field in embedFields)
								{
									// Remove reaction help text from prices field
									var fieldValue = field.Value;

									if (field.Name == "Best Market Board Prices")
									{
										fieldValue = fieldValue.Replace($"Use {HQEmote} to toggle HQ Only.", string.Empty);
										fieldValue = fieldValue.Replace($"Use {GilIEmote} to toggle sorting by Unit Price/Total Price.", string.Empty);
									}

									builder.AddField(new EmbedFieldBuilder()
										.WithName(field.Name)
										.WithValue(fieldValue));
								}
							}

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
			this.DiscordClient.ReactionAdded -= this.OnReactionAdded;
			return Task.CompletedTask;
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> incomingMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			try
			{
				Log.Write("Reaction Added Market Board Command", "Bot");

				// Don't react to your own reacts!
				if (reaction.UserId == this.DiscordClient.CurrentUser.Id)
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

				if (channel.Value is IMessageChannel guildChannel)
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
						Embed embed = await this.GetMarketBoardEmbed(mBWindow.ItemId, mBWindow.DataCentre, mBWindow.HqOnly, mBWindow.LowestByUnitPrice);
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
			public ActiveMBWindow(ulong userId, ulong itemId, XIVData.DataCentre dataCentre, bool? hqOnly, bool lowestByUnitPrice)
			{
				this.UserId = userId;
				this.ItemId = itemId;
				this.DataCentre = dataCentre;
				this.HqOnly = hqOnly;
				this.LowestByUnitPrice = lowestByUnitPrice;

				this.LastInteractedWith = DateTime.Now;
			}

			public ulong UserId { get; set; }
			public ulong ItemId { get; set; }
			public XIVData.DataCentre DataCentre { get; set; }
			public bool? HqOnly { get; set; }
			public bool LowestByUnitPrice { get; set; }
			public DateTime LastInteractedWith { get; set; }
		}
	}
}
