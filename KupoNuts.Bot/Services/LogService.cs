// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Utils;

	public class LogService : ServiceBase
	{
		public override Task Initialize()
		{
			Program.DiscordClient.UserJoined += this.DiscordClient_UserJoined;
			Program.DiscordClient.UserLeft += this.DiscordClient_UserLeft;
			Program.DiscordClient.UserBanned += this.DiscordClient_UserBanned;
			Program.DiscordClient.UserUnbanned += this.DiscordClient_UserUnbanned;

			return base.Initialize();
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.UserJoined -= this.DiscordClient_UserJoined;
			Program.DiscordClient.UserLeft -= this.DiscordClient_UserLeft;
			Program.DiscordClient.UserBanned -= this.DiscordClient_UserBanned;
			Program.DiscordClient.UserUnbanned -= this.DiscordClient_UserUnbanned;

			return base.Shutdown();
		}

		[Command("LogMe", Permissions.Administrators, "Test a user join log message.")]
		public async Task LogMe(SocketMessage message)
		{
			await this.PostMessage((SocketGuildUser)message.Author, Color.Purple, "Is testing");
		}

		private async Task DiscordClient_UserJoined(SocketGuildUser user)
		{
			await this.PostMessage(user, Color.Green, "Joined");
		}

		private async Task DiscordClient_UserLeft(SocketGuildUser user)
		{
			await this.PostMessage(user, Color.LightGrey, "Left");
		}

		private async Task DiscordClient_UserBanned(SocketUser user, SocketGuild guild)
		{
			await this.PostMessage(user, Color.Red, "Was Banned");
		}

		private async Task DiscordClient_UserUnbanned(SocketUser user, SocketGuild guild)
		{
			await this.PostMessage(user, Color.Orange, "Was Unbanned");
		}

		private async Task PostMessage(SocketUser user, Color color, string message)
		{
			if (!ulong.TryParse(Settings.Load().UserLogChannel, out ulong channelId))
				return;

			SocketTextChannel? channel = Program.DiscordClient.GetChannel(channelId) as SocketTextChannel;

			if (channel == null)
				return;

			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = color;
			builder.Title = user.Username + " " + message;
			builder.Timestamp = DateTimeOffset.Now;
			builder.ThumbnailUrl = user.GetAvatarUrl();

			if (user is SocketGuildUser guildUser)
			{
				builder.Title = guildUser.Nickname + " (" + user.Username + ") " + message;
				builder.AddField("Joined", TimeUtils.GetDateTimeString(guildUser.JoinedAt));
			}

			builder.AddField("Created", TimeUtils.GetDateTimeString(user.CreatedAt));

			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = "ID: " + user.Id;

			await channel.SendMessageAsync(null, false, builder.Build());
		}
	}
}
