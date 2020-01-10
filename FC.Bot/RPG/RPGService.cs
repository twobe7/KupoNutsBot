// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot.RPG
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.RPG;

	public class RPGService : ServiceBase
	{
		public static string NutEmoteStr = "<:kupo_nut:629887117819904020>";
		public static IEmote NutEmote = Emote.Parse(NutEmoteStr);

		private const double GenerationChance = 0.05;

		public override async Task Initialize()
		{
			await base.Initialize();

			Program.DiscordClient.MessageReceived += this.OnMessageReceived;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.MessageReceived -= this.OnMessageReceived;
			return base.Shutdown();
		}

		[Command("SendFC", Permissions.Everyone, "Sends kupo nuts to the specified user")]
		public async Task<(string, Embed)> SendNuts(CommandMessage message, IGuildUser destinationUser, int count = 1)
		{
			await Task.Delay(0);
			throw new NotImplementedException();
		}

		[Command("Inventory", Permissions.Everyone, "Shows your current profile")]
		[Command("Profile", Permissions.Everyone, "Shows your current profile")]
		public async Task<Embed> ShowProfile(CommandMessage message)
		{
			return await this.ShowProfile(message, message.Author);
		}

		[Command("Inventory", Permissions.Everyone, "Shows the profile of the specified user")]
		[Command("Profile", Permissions.Everyone, "Shows the profile of the specified user")]
		public async Task<Embed> ShowProfile(CommandMessage message, IGuildUser user)
		{
			await Task.Delay(0);
			throw new NotImplementedException();
		}

		[Command("FC", Permissions.Everyone, "Shows the kupo nut leader boards")]
		public async Task<Embed> ShowLeaders(CommandMessage message)
		{
			List<UserService.User> users = await UserService.GetAllUsers();

			users.Sort((UserService.User a, UserService.User b) =>
			{
				return -a.Nuts.CompareTo(b.Nuts);
			});

			IGuild guild = message.Guild;

			StringBuilder builder = new StringBuilder();
			int count = 0;
			foreach (UserService.User user in users)
			{
				if (user.DiscordGuildId != guild.Id)
					continue;

				count++;

				if (count > 10)
					break;

				if (user.Id == null)
					continue;

				IGuildUser discordUser = await guild.GetUserAsync(user.DiscordUserId);

				if (discordUser == null)
					continue;

				builder.Append(user.Nuts);
				builder.Append(" - ");
				builder.AppendLine(discordUser.GetName());
			}

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Title = "Kupo Nut Leaderboard";
			embedBuilder.Description = builder.ToString();
			return embedBuilder.Build();
		}

		[Command("Levels", Permissions.Everyone, "Shows the level leaderboards")]
		public async Task<Embed> ShowLevelLeaders(CommandMessage message)
		{
			List<UserService.User> users = await UserService.GetAllUsers();

			users.Sort((UserService.User a, UserService.User b) =>
			{
				return -a.Level.CompareTo(b.Level);
			});

			IGuild guild = message.Guild;

			StringBuilder builder = new StringBuilder();
			int count = 0;
			foreach (UserService.User user in users)
			{
				if (user.DiscordGuildId != guild.Id)
					continue;

				count++;

				if (count > 10)
					break;

				if (user.Id == null)
					continue;

				IGuildUser discordUser = await guild.GetUserAsync(user.DiscordUserId);

				if (discordUser == null)
					continue;

				builder.Append(user.Level);
				builder.Append(" - ");
				builder.AppendLine(discordUser.GetName());
			}

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Title = "Level Leaderboard";
			embedBuilder.Description = builder.ToString();
			return embedBuilder.Build();
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			try
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
					if (roll < GenerationChance)
					{
						IGuildUser toUser = message.GetAuthor();

						Log.Write(toUser.GetName() + " Found a Kupo Nut with message: \"" + message.Content + "\"", "Bot");

						UserService.User user = await UserService.GetUser(toUser);
						user.Nuts++;
						await UserService.SaveUser(user);
					}
				}
			}
			catch (Discord.Net.HttpException)
			{
				// in case the discord request has failed, just abort out.
				// this can happen if a message is deleted or edited before we call GetMessageAsync.
				return;
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
