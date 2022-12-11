// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;
	using System.Text;

	public static class Log
	{
		public delegate void LogEvent(string str);

		public static event LogEvent? MessageLogged;

		public static event LogEvent? ExceptionLogged;

		public static void Write(string message, string category)
		{
			string str = $"[{DateTime.Now:HH:mm:ss}][{category}] {message}";
			Console.WriteLine(str);
			MessageLogged?.Invoke(str);
		}

		public static void Write(Exception? ex)
		{
			if (ex == null)
				return;

			StringBuilder builder = new StringBuilder();
			while (ex != null)
			{
				builder.Append(ex.GetType());
				builder.Append(" - ");
				builder.AppendLine(ex.Message);
				builder.AppendLine(ex.StackTrace);
				builder.AppendLine();

				ex = ex.InnerException;
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(builder.ToString());
			Console.ForegroundColor = ConsoleColor.White;

			ExceptionLogged?.Invoke(builder.ToString());
		}

		////public static async System.Threading.Tasks.Task LogExceptionToDiscordChannel(Exception exception, string messageContent, string? guild = null, string? user = null)
		////{
		////	// Get Settings - check if both bot server and exception channel is given
		////	Settings settings = Settings.Load();
		////	if (!string.IsNullOrWhiteSpace(settings?.BotDiscordServer) && !string.IsNullOrWhiteSpace(settings?.BotLogExceptionsChannel))
		////	{
		////		// Get the guild
		////		Discord.WebSocket.SocketGuild kupoNutsGuild = Program.DiscordClient.GetGuild(ulong.Parse(settings.BotDiscordServer));

		////		if (kupoNutsGuild == null)
		////			throw new Exception("Unable to access guild");

		////		// Post message
		////		string exceptionMessage = $"Server: {guild ?? "Unknown"}\nUser: {user ?? "Unknown"}\nMessage: {messageContent}.\n`{exception}`";
		////		await kupoNutsGuild.GetTextChannel(ulong.Parse(settings.BotLogExceptionsChannel)).SendMessageAsync(exceptionMessage);
		////	}
		////}
	}
}
