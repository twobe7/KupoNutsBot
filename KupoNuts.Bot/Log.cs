// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot
{
	using System;
	using System.Text;
	using Discord;
	using Discord.WebSocket;

	public static class Log
	{
		public static void Write(string message)
		{
			Console.WriteLine("[Bot] " + message);
		}

		public static void Write(Exception? ex)
		{
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

			if (Program.DiscordClient != null)
			{
				try
				{
					string? idStr = Settings.Load().LogChannel;
					if (idStr != null)
					{
						ulong id = ulong.Parse(idStr);
						SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(id);
						EmbedBuilder enbedBuilder = new EmbedBuilder();
						enbedBuilder.Color = Color.Red;
						enbedBuilder.Title = "Kupo Nut Bot encountered an error";
						enbedBuilder.Description = builder.ToString();
						enbedBuilder.Timestamp = DateTimeOffset.UtcNow;
						channel.SendMessageAsync(null, false, enbedBuilder.Build());
					}
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
