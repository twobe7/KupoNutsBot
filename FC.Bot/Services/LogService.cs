// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Utils;

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

		public override async Task Shutdown()
		{
			Program.DiscordClient.UserJoined -= this.DiscordClient_UserJoined;
			Program.DiscordClient.UserLeft -= this.DiscordClient_UserLeft;
			Program.DiscordClient.UserBanned -= this.DiscordClient_UserBanned;
			Program.DiscordClient.UserUnbanned -= this.DiscordClient_UserUnbanned;

			await base.Shutdown();
		}

		private static async Task<SocketTextChannel?> GetChannel(ulong guildId)
		{
			GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guildId);
			if (string.IsNullOrEmpty(settings.LogChannel))
				return null;

			ulong channelId = ulong.Parse(settings.LogChannel);
			return Program.DiscordClient.GetChannel(channelId) as SocketTextChannel;
		}

		private async Task DiscordClient_UserJoined(SocketGuildUser user)
		{
			await this.PostMessage(user.Guild, user, Color.Green, "Joined");
		}

		private async Task DiscordClient_UserLeft(SocketGuild guild, SocketUser user)
		{
			if (user is IGuildUser guildUser)
			{
				await this.PostMessage(guild, guildUser, Color.LightGrey, "Left");
			}
			else
			{
				throw new Exception("User is not a guild user: " + user);
			}
		}

		private async Task DiscordClient_UserBanned(SocketUser user, SocketGuild guild)
		{
			if (user is IGuildUser guildUser)
			{
				await this.PostMessage(guild, guildUser, Color.Red, "Was Banned");
			}
			else
			{
				throw new Exception("User is not a guild user: " + user);
			}
		}

		private async Task DiscordClient_UserUnbanned(SocketUser user, SocketGuild guild)
		{
			if (user is IGuildUser guildUser)
			{
				await this.PostMessage(guild, guildUser, Color.Orange, "Was Unbanned");
			}
			else
			{
				throw new Exception("User is not a guild user: " + user);
			}
		}

		private async Task PostMessage(IGuild guild, IGuildUser user, Color color, string message)
		{
			SocketTextChannel? channel = await GetChannel(guild.Id);

			if (channel == null)
				return;

			// don't push logs to different guilds.
			if (user.GuildId != channel.Guild.Id)
				return;

			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = color;
			builder.Title = user.Username + " " + message + " " + user.Guild.Name;
			builder.Timestamp = DateTimeOffset.Now;
			builder.ThumbnailUrl = user.GetAvatarUrl();

			if (user is SocketGuildUser guildUser)
			{
				builder.Title = guildUser.Nickname + " (" + user.Username + ") " + message;
				builder.AddField("Joined", TimeUtils.GetDateString(guildUser.JoinedAt), true);
			}

			builder.AddField("Created", TimeUtils.GetDateString(user.CreatedAt), true);

			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = "ID: " + user.Id;

			await channel.SendMessageAsync(null, false, builder.Build());
		}
	}
}
