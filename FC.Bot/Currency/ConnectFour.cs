// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Currency
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;

	public class ConnectFour
	{
		private const byte RowsCount = 6;
		private const byte ColumnsCount = 7;

		private const string LeftDivider = "|" + Utils.Characters.DoubleSpace;
		private const string VerticalDivider = Utils.Characters.DoubleSpace + "|" + Utils.Characters.DoubleSpace;
		private const string HorizontalDivider = Utils.Characters.DoubleSpace + Utils.Characters.Space + "----"
			+ Utils.Characters.Tab + "----"
			+ Utils.Characters.Tab + "----"
			+ Utils.Characters.Tab + "----"
			+ Utils.Characters.Tab + "----"
			+ Utils.Characters.Tab + "----"
			+ Utils.Characters.Tab + "----";

		private static List<Game>? activeGames;

		public enum Piece
		{
			[Description(":white_circle:")]
			None,
			[Description(":red_circle:")]
			Red,
			[Description(":yellow_circle:")]
			Yellow,
		}

		public async Task<Task> StartGame(IInteractionContext ctx, IGuildUser challenger)
		{
			// New game
			var newGame = new Game(ctx, challenger);
			if (activeGames == null)
			{
				activeGames = new List<Game> { newGame };
			}
			else
			{
				activeGames.Add(newGame);
			}

			// Send starting board
			await ctx.Interaction.FollowupAsync(
				text: newGame.GetPostMessage(),
				embeds: new Embed[] { newGame.GetEmbed() },
				components: newGame.GetComponents());

			// Stop game
			_ = Task.Run(async () => await StopInactiveGame(newGame));

			return Task.CompletedTask;
		}

		public async Task<Task> Play(IInteractionContext ctx, byte column)
		{
			// Get current game
			Game? game = null;
			if (ctx.Interaction is SocketMessageComponent interaction)
			{
				var messageId = interaction.Message.Interaction.Id;
				game = activeGames?.FirstOrDefault(x => x.Interaction.Id == messageId);

				if (ctx.User.Id != game?.CurrentPlayerId)
				{
					await interaction.FollowupAsync(text: "You're not the current player. Please wait.", ephemeral: true);
					return Task.CompletedTask;
				}
			}

			if (game == null)
				throw new InvalidOperationException("No game found!");

			// Play token
			game.PlayToken(column);

			// Check for winner
			var winner = game.CheckForWinner();
			if (winner != Piece.None)
			{
				// Send update to Discord
				await ctx.Interaction.ModifyOriginalResponseAsync(x =>
				{
					x.Content = game.GetPostMessage(winner);
					x.Embed = game.GetWinEmbed(winner);
					x.Components = null;
				});

				// Remove the game from active Games
				activeGames?.Remove(game);

				return Task.CompletedTask;
			}

			// Send update to Discord
			await ctx.Interaction.ModifyOriginalResponseAsync(x =>
			{
				x.Embed = game.GetEmbed();
				x.Components = game.GetComponents();
			});

			return Task.CompletedTask;
		}

		private static async Task<Task> StopInactiveGame(Game game)
		{
			// Keep delaying until not active
			while ((DateTime.Now - game.LastMove).TotalMinutes < 30)
			{
				// Give the user time to select move
				await Task.Delay(10000);
			}

			await Task.Delay(3000);

			// Remove the game
			activeGames?.Remove(game);

			// Replace with closed window
			await game.Interaction.ModifyOriginalResponseAsync(x =>
			{
				x.Content = game.GetPostMessage(Piece.None, true);
				x.Embed = game.GetTieEmbed();
				x.Components = null;
			});

			return Task.CompletedTask;
		}

		public class Game
		{
			public Game(IInteractionContext ctx, IGuildUser playerTwo)
			{
				this.Interaction = ctx.Interaction;
				this.StartTime = DateTime.Now;
				this.GameBoard = new Piece[RowsCount, ColumnsCount];
				this.CurrentTurn = Piece.Red;
				this.LastMove = DateTime.Now;

				this.PlayerOne = new Player(ctx.User.Id, (ctx.User as IGuildUser)?.DisplayName ?? ctx.User.GlobalName);
				this.PlayerTwo = new Player(playerTwo.Id, playerTwo.DisplayName, Piece.Yellow);
			}

			public IDiscordInteraction Interaction { get; set; }
			public Piece[,] GameBoard { get; set; }
			public DateTime StartTime { get; set; }
			public Player PlayerOne { get; set; }
			public Player PlayerTwo { get; set; }
			public Piece CurrentTurn { get; set; }
			public DateTime LastMove { get; set; }

			public string TurnMessage => this.CurrentTurn switch
			{
				Piece.Red => "Red Turn",
				Piece.Yellow => "Yellow Turn",
				_ => throw new Exception("CurrentTurn not a valid value"),
			};

			public ulong CurrentPlayerId => this.PlayerOne.Piece == this.CurrentTurn ? this.PlayerOneId : this.PlayerTwoId;

			private ulong PlayerOneId => this.PlayerOne.Id;
			private ulong PlayerTwoId => this.PlayerTwo.Id;

			public Embed GetEmbed()
			{
				return new EmbedBuilder
				{
					Title = $"Connect Four - {this.TurnMessage}",
					Color = Color.Default,
					Description = this.GetBoard(),
				}.Build();
			}

			public Embed GetWinEmbed(Piece winner)
			{
				return new EmbedBuilder
				{
					Title = $"Connect Four - {(winner == Piece.Red ? "RED WINS" : "YELLOW WINS")}",
					Color = winner == Piece.Red ? new Color(221, 46, 68) : new Color(253, 203, 88),
					Description = this.GetBoard(),
				}.Build();
			}

			public Embed GetTieEmbed()
			{
				return new EmbedBuilder
				{
					Title = "Connect Four - TIE",
					Color = Color.Default,
					Description = this.GetBoard(),
				}.Build();
			}

			public string GetBoard()
			{
				StringBuilder board = new ();

				for (int i = 0; i < this.GameBoard.GetLength(0); i++)
				{
					board.Append(LeftDivider);

					for (int j = 0; j < this.GameBoard.GetLength(1); j++)
					{
						board.Append(this.GameBoard[i, j].ToDisplayString());
						board.Append(VerticalDivider);
					}

					board.AppendLine();
					board.AppendLine(HorizontalDivider);
				}

				return board.ToString();
			}

			public MessageComponent GetComponents()
			{
				return new ComponentBuilder()
					.WithButton(label: "1", customId: "connectFourResponse-0", disabled: this.GetTopPosition(0) == byte.MaxValue)
					.WithButton(label: "2", customId: "connectFourResponse-1", disabled: this.GetTopPosition(1) == byte.MaxValue)
					.WithButton(label: "3", customId: "connectFourResponse-2", disabled: this.GetTopPosition(2) == byte.MaxValue)
					.WithButton(label: "4", customId: "connectFourResponse-3", disabled: this.GetTopPosition(3) == byte.MaxValue)
					.WithButton(label: "5", customId: "connectFourResponse-4", disabled: this.GetTopPosition(4) == byte.MaxValue)
					.WithButton(label: "6", customId: "connectFourResponse-5", disabled: this.GetTopPosition(5) == byte.MaxValue)
					.WithButton(label: "7", customId: "connectFourResponse-6", disabled: this.GetTopPosition(6) == byte.MaxValue)
					.Build();
			}

			public void PlayToken(byte column, bool changeTurn = true)
			{
				var row = this.GetTopPosition(column);
				this.GameBoard.SetValue(this.CurrentTurn, row, column);

				if (changeTurn)
					this.ChangeTurn();

				this.LastMove = DateTime.Now;
			}

			public Piece CheckForWinner()
			{
				Piece winningPiece;
				for (byte i = 0; i < ColumnsCount; i++)
				{
					var column = Enumerable.Range(0, RowsCount).Select(m => this.GameBoard[m, i]).ToArray();
					winningPiece = HasFourInRow(column);
					if (winningPiece != Piece.None)
						return winningPiece;
				}

				for (byte i = 0; i < RowsCount; i++)
				{
					var row = Enumerable.Range(0, ColumnsCount).Select(m => this.GameBoard[i, m]).ToArray();
					winningPiece = HasFourInRow(row);
					if (winningPiece != Piece.None)
						return winningPiece;
				}

				var diagonals = new[]
				{
					// negative slope
					new[] { this.GameBoard[2, 0], this.GameBoard[3, 1], this.GameBoard[4, 2], this.GameBoard[5, 3] },
					new[] { this.GameBoard[1, 0], this.GameBoard[2, 1], this.GameBoard[3, 2], this.GameBoard[4, 3], this.GameBoard[5, 4] },
					new[] { this.GameBoard[0, 0], this.GameBoard[1, 1], this.GameBoard[2, 2], this.GameBoard[3, 3], this.GameBoard[4, 4], this.GameBoard[5, 5] },
					new[] { this.GameBoard[0, 1], this.GameBoard[1, 2], this.GameBoard[2, 3], this.GameBoard[3, 4], this.GameBoard[4, 5], this.GameBoard[5, 6] },
					new[] { this.GameBoard[0, 2], this.GameBoard[1, 3], this.GameBoard[2, 4], this.GameBoard[3, 5], this.GameBoard[4, 6] },
					new[] { this.GameBoard[0, 3], this.GameBoard[1, 4], this.GameBoard[2, 5], this.GameBoard[3, 6] },

					// positive slope
					new[] { this.GameBoard[3, 0], this.GameBoard[2, 1], this.GameBoard[1, 2], this.GameBoard[0, 3] },
					new[] { this.GameBoard[4, 0], this.GameBoard[3, 1], this.GameBoard[2, 2], this.GameBoard[1, 3], this.GameBoard[0, 4] },
					new[] { this.GameBoard[5, 0], this.GameBoard[4, 1], this.GameBoard[3, 2], this.GameBoard[2, 3], this.GameBoard[1, 4], this.GameBoard[0, 5] },
					new[] { this.GameBoard[5, 1], this.GameBoard[4, 2], this.GameBoard[3, 3], this.GameBoard[2, 4], this.GameBoard[1, 5], this.GameBoard[0, 6] },
					new[] { this.GameBoard[5, 2], this.GameBoard[4, 3], this.GameBoard[3, 4], this.GameBoard[2, 5], this.GameBoard[1, 6] },
					new[] { this.GameBoard[5, 3], this.GameBoard[4, 4], this.GameBoard[3, 5], this.GameBoard[2, 6] },
				};

				foreach (var diagonal in diagonals)
				{
					winningPiece = HasFourInRow(diagonal);
					if (winningPiece != Piece.None)
						return winningPiece;
				}

				return Piece.None;
			}

			public string GetPostMessage(Piece winner = Piece.None, bool isTie = false)
			{
				var playerOne = isTie || winner == Piece.Yellow
					? $"~~{this.PlayerOne.Name}~~"
					: this.PlayerOne.Name;

				var playerTwo = isTie || winner == Piece.Red
					? $"~~{this.PlayerTwo.Name}~~"
					: this.PlayerTwo.Name;

				return $"{playerOne} vs {playerTwo}";
			}

			private static Piece HasFourInRow(Piece[] pieces)
			{
				byte connected = 0;
				var lastPiece = Piece.None;
				foreach (var piece in pieces)
				{
					if (piece == Piece.None)
					{
						connected = 0;
						continue;
					}

					if (piece != lastPiece)
					{
						connected = 1;
						lastPiece = piece;
						continue;
					}

					connected++;
					if (connected == 4)
					{
						return lastPiece;
					}
				}

				return Piece.None;
			}

			private byte GetTopPosition(byte column)
			{
				for (int i = this.GameBoard.GetLength(0) - 1; i >= 0; i--)
				{
					if (this.GameBoard[i, column] == Piece.None)
						return (byte)i;
				}

				return byte.MaxValue;
			}

			private void ChangeTurn()
			{
				this.CurrentTurn = this.CurrentTurn switch
				{
					Piece.Red => Piece.Yellow,
					Piece.Yellow => Piece.Red,
					_ => throw new Exception("CurrentTurn not a valid value"),
				};
			}

			public class Player
			{
				public Player(ulong id, string name, Piece piece = Piece.Red)
				{
					this.Id = id;
					this.Name = name;
					this.Piece = piece;
				}

				public ulong Id { get; set; }
				public string Name { get; set; } = string.Empty;
				public Piece Piece { get; set; }
			}
		}
	}
}
