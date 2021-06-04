﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Currency
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;

	public class Blackjack
	{
		private static readonly List<IEmote> Reactions = new List<IEmote>()
		{
			Emote.Parse("<:hit:838773536235061275>"),
			Emote.Parse("<:stand:838773499349827664>"),
		};

		private static readonly IEmote FaceDown = new Emoji("🟦");

		private static readonly List<IEmote> Deck = new List<IEmote>()
		{
			new Emoji(@"🇦"),
			new Emoji(":two:"),
			new Emoji(":three:"),
			new Emoji(":four:"),
			new Emoji(":five:"),
			new Emoji(":six:"),
			new Emoji(":seven:"),
			new Emoji(":eight:"),
			new Emoji(":nine:"),
			new Emoji("🔟"),
			new Emoji("🇯"),
			new Emoji("🇶"),
			new Emoji("🇰"),
		};

		private static ActiveGame? activeGame;

		private uint betAmount = 10;

		public Blackjack()
		{
			this.betAmount = 10;
		}

		public Blackjack(uint betAmount)
		{
			this.betAmount = betAmount;
		}

		public async Task<Task> StartBlackjack(CommandMessage message)
		{
			// Only allow one game to be played at a time
			if (activeGame != null)
			{
				RestUserMessage rMessage = await message.Channel.SendMessageAsync("Hand has already been dealt. Please wait, _kupo!_");

				await Task.Delay(2000);

				await rMessage.DeleteAsync();
				await message.Channel.DeleteMessageAsync(message.Id);

				return Task.CompletedTask;
			}

			// New game
			activeGame = new ActiveGame(message.Author.Id, this.betAmount);

			// User already won - what luck
			if (activeGame.UserHandValue == 21)
			{
				// Final builder information
				EmbedBuilder builder = GetEmbedBuilder(true, true);
				RestUserMessage? bjMessage = await message.Channel.SendMessageAsync(null, false, builder.Build(), messageReference: message.MessageReference);

				activeGame = null;
				await Task.Delay(3000);

				// Stop game
				_ = Task.Run(async () => await StopBlackjack(bjMessage));
			}
			else
			{
				// Initial builder information
				EmbedBuilder builder = GetEmbedBuilder();

				// First deal
				RestUserMessage? bjMessage = await message.Channel.SendMessageAsync(null, false, builder.Build(), messageReference: message.MessageReference);

				// Update active game
				activeGame.MessageId = bjMessage.Id;

				// Add reacts
				await bjMessage.AddReactionsAsync(Reactions.ToArray());

				// Handle reacts
				Program.DiscordClient.ReactionAdded += OnReactionAdded;

				// Stop game after no response
				_ = Task.Run(async () => await StopBlackjack(bjMessage));
			}

			return Task.CompletedTask;
		}

		private static async void PlayDealerHand(IUserMessage message, SocketGuildChannel guildChannel, SocketReaction reaction)
		{
			// No active game, no handling
			if (activeGame == null)
				return;

			// Show dealer's second card
			await message.ModifyAsync(x => x.Embed = GetEmbedBuilder(true).Build());

			while (!activeGame.DealerIsStanding)
			{
				activeGame.DealCardToDealer();
				await message.ModifyAsync(x => x.Embed = GetEmbedBuilder(true).Build());
			}

			// Payout
			if (activeGame.UserWon)
			{
				User user = await UserService.GetUser(guildChannel.Guild.Id, reaction.UserId);
				user.UpdateTotalKupoNuts(activeGame.Payout());
			}

			// Game finished - Perform last build, remove reactions, and clear active game
			await message.ModifyAsync(x => x.Embed = GetEmbedBuilder(true, true).Build());
			await message.RemoveAllReactionsAsync();
			activeGame = null;
		}

		private static EmbedBuilder GetEmbedBuilder(bool revealDealerHand = false, bool finalBuild = false)
		{
			// Initial builder information
			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = "Blackjack";
			builder.Color = Color.Gold;

			if (activeGame == null)
				return builder;

			if (finalBuild)
			{
				builder.Description = activeGame.ResultMessage;
			}

			EmbedFieldBuilder dealerHandField = new EmbedFieldBuilder().WithName("Dealer's Hand");

			if (revealDealerHand || activeGame.DealerHandValue == 21)
			{
				dealerHandField.Name += " - " + activeGame.DealerHandValue;

				foreach (IEmote card in activeGame.DealerHand)
					dealerHandField.Value += card + Utils.Characters.Tab;
			}
			else
			{
				dealerHandField.Value += activeGame.DealerHand[0].ToString();
				dealerHandField.Value += Utils.Characters.Tab + FaceDown.ToString();
			}

			builder.AddField(dealerHandField);

			// Build user's hand
			EmbedFieldBuilder userHandField = new EmbedFieldBuilder().WithName("User's Hand - " + activeGame.UserHandValue);

			// Display user hand
			foreach (IEmote card in activeGame.UserHand)
				userHandField.Value += card + Utils.Characters.Tab;

			builder.AddField(userHandField);

			return builder;
		}

		private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> incomingMessage, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				// Don't react to your own reacts!
				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return;

				// No active game, no handling
				if (activeGame == null)
					return;

				// Only handle reacts to blackjack game
				if (activeGame.MessageId != incomingMessage.Id)
					return;

				// Only handle reacts from the original user, remove the reaction
				if (activeGame.UserId != reaction.UserId)
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					return;
				}

				// Only handle relevant reacts
				if (!Reactions.Contains(reaction.Emote))
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					return;
				}

				if (channel is SocketGuildChannel guildChannel)
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					// Play
					switch (reaction.Emote.Name.ToLower())
					{
						case "hit":
							activeGame.DealCardToUser();
							break;
						case "stand":
							activeGame.UserIsStanding = true;
							break;
						default:
							break;
					}

					if (activeGame.UserIsStanding)
					{
						PlayDealerHand(message, guildChannel, reaction);
					}
					else
					{
						// Build and send
						await message.ModifyAsync(x => x.Embed = GetEmbedBuilder().Build());

						if (activeGame.UserBusted || activeGame.UserHandValue == 21)
							PlayDealerHand(message, guildChannel, reaction);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private static async Task<Task> StopBlackjack(RestUserMessage message)
		{
			// Keep delaying until not active
			while (activeGame != null && (DateTime.Now - activeGame.LastInteractedWith).TotalSeconds < 30)
			{
				// Give the user time to select move
				await Task.Delay(4000);
			}

			await Task.Delay(3000);

			// Remove the game
			activeGame = null;

			// Stop wait for reaction
			Program.DiscordClient.ReactionAdded -= OnReactionAdded;

			// Remove reactions and replace with closed window
			await message.RemoveAllReactionsAsync();

			return Task.CompletedTask;
		}

		public class ActiveGame
		{
			public ActiveGame(ulong userId, uint bet)
			{
				this.UserId = userId;
				this.LastInteractedWith = DateTime.Now;
				this.BetAmount = bet;

				// Initial draw
				this.DealCardToDealer(2);
				this.DealCardToUser(2);
			}

			public ulong MessageId { get; set; }
			public ulong UserId { get; set; }
			public List<IEmote> DealerHand { get; set; } = new List<IEmote>();
			public List<IEmote> UserHand { get; set; } = new List<IEmote>();
			public DateTime LastInteractedWith { get; set; }

			public uint BetAmount { get; set; }

			public bool DealerIsStanding => this.UserBusted || (this.DealerHandValue > 17 && this.DealerHandValue > this.UserHandValue);
			public bool UserIsStanding { get; set; }
			public bool DealerBusted => this.DealerHandValue > 21;
			public bool UserBusted => this.UserHandValue > 21;
			public bool UserWon => !this.UserBusted && (this.UserHandValue > this.DealerHandValue || this.DealerBusted);

			public int DealerHandValue => this.GetHandValue(this.DealerHand);
			public int UserHandValue => this.GetHandValue(this.UserHand);

			public string ResultMessage
			{
				get
				{
					if (!this.UserWon)
					{
						return "You Lose!\nMaybe next time, _kupo!_";
					}

					string payout = (this.Payout() - this.BetAmount).ToString();

					return this.UserHandValue == 21
						? $"Blackjack, _kupo!_\nYou win {payout} Kupo Nuts!"
						: $"You Win!\nThat's {payout} Kupo Nuts for you!";
				}
			}

			public void DealCardToUser(int cardsToDraw = 1)
			{
				for (int i = 0; i < cardsToDraw; i++)
					this.UserHand.Add(this.DrawCard());
			}

			public void DealCardToDealer(int cardsToDraw = 1)
			{
				for (int i = 0; i < cardsToDraw; i++)
					this.DealerHand.Add(this.DrawCard());
			}

			public int Payout()
			{
				if (!this.UserWon)
					return 0;

				return this.UserHandValue == 21
					? (int)(this.BetAmount * 2.5)
					: (int)(this.BetAmount * 2);
			}

			private IEmote DrawCard()
			{
				return Deck[new Random().Next(13)];
			}

			private int GetHandValue(List<IEmote> hand)
			{
				int value = 0;
				bool aceCheck = false;
				foreach (IEmote card in hand)
				{
					int pos = Deck.FindIndex(x => x.Name == card.Name);
					if (pos == 0)
					{
						if (aceCheck)
						{
							value += 1;
						}
						else
						{
							value += 11;
							aceCheck = true;
						}
					}
					else if (pos >= 9)
					{
						value += 10;
					}
					else
					{
						value += pos + 1;
					}

					// Check if busted due to high ace
					if (value > 21 && aceCheck)
					{
						value -= 10;
						aceCheck = false;
					}
				}

				return value;
			}
		}
	}
}
