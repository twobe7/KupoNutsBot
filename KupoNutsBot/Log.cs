// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Discord;
	using Discord.WebSocket;

	public static class Log
	{
		public static void Write(string message)
		{
			Console.WriteLine(message);
		}

		public static void Write(Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			Console.ForegroundColor = ConsoleColor.White;

			if (Program.DiscordClient != null)
			{
				try
				{
					SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(Database.Instance.LogChannel);
					EmbedBuilder builder = new EmbedBuilder();
					builder.Color = Color.Red;
					builder.Title = "Kupo Nut Bot encountered an error";
					builder.Description = ex.Message;
					builder.AddField("Stack", ex.StackTrace);
					builder.Timestamp = DateTimeOffset.UtcNow;
					channel.SendMessageAsync(null, false, builder.Build());
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
