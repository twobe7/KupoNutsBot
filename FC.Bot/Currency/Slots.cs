// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Currency
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using FC.Bot.Services;

	public class Slots
	{
		public const string KupoNut = "<:kupo_nut:815575569482776607>";
		public const string Strawberry = ":strawberry:";
		public const string Cherries = ":cherries:";
		public const string Banana = ":banana:";
		public const string Kiwi = ":kiwi:";
		public const string Lemon = ":lemon:";

		private const string VerticalDivider = Utils.Characters.Tab + "|" + Utils.Characters.Tab;
		private const string HorizontalDivider = "-----" + Utils.Characters.Space + Utils.Characters.Space + Utils.Characters.Space + "--------" + Utils.Characters.Space + Utils.Characters.Space + "-------";

		private static readonly List<string> PossibleSlotItem =
		[
			KupoNut, Strawberry, Cherries, Banana, Kiwi, Lemon,
		];

		private static readonly Dictionary<int, List<string>> WinningSlotCombinations = new Dictionary<int, List<string>>()
		{
			{ 0, new List<string>() { KupoNut, KupoNut, KupoNut } },
			{ 1, new List<string>() { Strawberry, Strawberry, Strawberry } },
			{ 2, new List<string>() { Cherries, Cherries, Cherries } },
			{ 3, new List<string>() { Kiwi, Kiwi, Kiwi } },
			{ 4, new List<string>() { Banana, Banana, Banana } },
			{ 5, new List<string>() { Lemon, Lemon, Lemon } },
		};

		private enum WinType
		{
			[Description("You Lose!\nMaybe next time, _kupo!_")]
			Lose = 0,
			[Description("You Win!\nThat's 100 Kupo Nuts for you!")]
			Win = 1,
			[Description("Jackpot, _kupo!_\nYou win 1000 Kupo Nuts!")]
			Jackpot = 2,
		}

		public async Task<Task> StartSlot(IInteractionContext ctx)
		{
			// Initial builder information
			EmbedBuilder builder = new()
			{
				Title = "Spinning",
				Color = Color.Gold,
			};

			// Get board and final combination
			Dictionary<int, List<string>> board = this.GetSlotBoard();

			// Update board
			this.SpinSlotGrid(builder, board, 0, out WinType winType);

			// First layout
			await ctx.Interaction.FollowupAsync(null, new Embed[] { builder.Build() });

			// Make it spin
			for (int spin = 1; spin < 6; spin++)
			{
				// Discord doesn't like it when we edit embed to soon after posting them, as the edit
				// sometimes doesn't 'stick'.
				await Task.Delay(400);

				builder.Title += ".";

				this.SpinSlotGrid(builder, board, spin, out winType);

				// Final spin, use winning message for title
				if (spin == 5)
				{
					builder.Title = winType.ToDisplayString();

					if (winType == WinType.Win)
					{
						User user = await UserService.GetUser(ctx.Guild.Id, ctx.User.Id);
						await user.UpdateTotalKupoNuts(CurrencyService.DefaultBetAmount * 11, receivedAmount: CurrencyService.DefaultBetAmount * 10);
					}
					else if (winType == WinType.Jackpot)
					{
						User user = await UserService.GetUser(ctx.Guild.Id, ctx.User.Id);
						await user.UpdateTotalKupoNuts(CurrencyService.DefaultBetAmount * 101, receivedAmount: CurrencyService.DefaultBetAmount * 100);
					}
				}

				await ctx.Interaction.ModifyOriginalResponseAsync(x => x.Embed = builder.Build());
			}

			return Task.CompletedTask;
		}

		private Dictionary<int, List<string>> GetSlotBoard()
		{
			Dictionary<int, List<string>> board = [];

			// Get winning line
			List<string> finalLine;
			int seed = new Random().Next(50);

			if (seed == 0)
			{
				// Jackpot
				finalLine = WinningSlotCombinations[0];
			}
			else if (seed < 6)
			{
				// Won
				finalLine = WinningSlotCombinations[new Random().Next(1, WinningSlotCombinations.Count)];
			}
			else
			{
				// Lost... maybe
				finalLine = [PossibleSlotItem.GetRandom(), PossibleSlotItem.GetRandom(), PossibleSlotItem.GetRandom(),];
			}

			// Build rest of board
			for (int c = 0; c < 3; c++)
			{
				List<string> column = [];

				for (int i = 0; i < 10; i++)
				{
					if ((c == 0 && i == 3) || (c == 1 && i == 4) || (c == 2 && i == 5))
					{
						column.Add(finalLine[c]);
					}
					else
					{
						column.Add(PossibleSlotItem.GetRandom());
					}
				}

				board.Add(c, column);
			}

			return board;
		}

		private void SpinSlotGrid(EmbedBuilder builder, Dictionary<int, List<string>> board, int spin, out WinType winType)
		{
			StringBuilder description = new();

			int leftSpin = spin > 2 ? 3 : spin;
			int middleSpin = spin > 3 ? 4 : spin;
			int rightSpin = spin > 4 ? 5 : spin;

			string finalPosRight = board[2][rightSpin++];
			string finalPosMiddle = board[1][middleSpin++];
			string finalPosLeft = board[0][leftSpin++];

			winType = (finalPosLeft == finalPosMiddle && finalPosMiddle == finalPosRight)
								? (finalPosLeft == KupoNut ? WinType.Jackpot : WinType.Win)
								: WinType.Lose;

			description.Insert(0, finalPosRight);
			description.Insert(0, finalPosMiddle + VerticalDivider);
			description.Insert(0, finalPosLeft + VerticalDivider);

			description.Insert(0, Environment.NewLine);
			description.Insert(0, HorizontalDivider);
			description.Insert(0, Environment.NewLine);

			for (int row = 0; row < 3; row++)
			{
				description.Insert(0, board[2][rightSpin++]);
				description.Insert(0, board[1][middleSpin++] + VerticalDivider);
				description.Insert(0, board[0][leftSpin++] + VerticalDivider);

				description.Insert(0, Environment.NewLine);
			}

			builder.Description = description.ToString();
		}
	}
}
