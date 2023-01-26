// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Utils
{
	using System;
	using Discord;

	public static class Logger
	{
		public static async System.Threading.Tasks.Task LogExceptionToDiscordChannel(Exception exception, Commands.CommandMessage message)
		{
			// Get Settings - check if both bot server and exception channel is given
			Settings settings = Settings.Load();
			if (!string.IsNullOrWhiteSpace(settings?.BotDiscordServer) && !string.IsNullOrWhiteSpace(settings?.BotLogExceptionsChannel))
			{
				// Get the guild
				Discord.WebSocket.SocketGuild botGuild = Program.DiscordClient.GetGuild(ulong.Parse(settings.BotDiscordServer));

				if (botGuild == null)
					throw new Exception("Unable to access guild");

				// Post message
				string exceptionMessage = $"Server: {message.Guild.Name}\nUser: {message.Author.GetName()}\nMessage: {message.Message.Content}.\n`{exception}`";
				await botGuild.GetTextChannel(ulong.Parse(settings.BotLogExceptionsChannel)).SendMessageAsync(exceptionMessage.Truncate(2000));
			}
		}

		public static async System.Threading.Tasks.Task LogExceptionToDiscordChannel(Exception exception, string messageContent, string? guild = null, string? user = null)
		{
			// Get Settings - check if both bot server and exception channel is given
			Settings settings = Settings.Load();
			if (!string.IsNullOrWhiteSpace(settings?.BotDiscordServer) && !string.IsNullOrWhiteSpace(settings?.BotLogExceptionsChannel))
			{
				// Get the guild
				Discord.WebSocket.SocketGuild botGuild = Program.DiscordClient.GetGuild(ulong.Parse(settings.BotDiscordServer));

				if (botGuild == null)
					throw new Exception("Unable to access guild");

				// Post message
				string exceptionMessage = $"Server: {guild ?? "Unknown"}\nUser: {user ?? "Unknown"}\nMessage: {messageContent}.\n`{exception}`";
				await botGuild.GetTextChannel(ulong.Parse(settings.BotLogExceptionsChannel)).SendMessageAsync(exceptionMessage.Truncate(2000));
			}
		}
	}
}
