// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server
{
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;

	public static class DiscordAPI
	{
		public static DiscordSocketClient Client
		{
			get;
			private set;
		}

		public static async Task Start()
		{
			string token = Settings.Load().Token;

			if (string.IsNullOrEmpty(token))
			{
				Log.Write("No token set. Please set a token in the Database file", "Bot");
			}
			else
			{
				Client = new DiscordSocketClient();

				bool ready = false;
				Client.Log += LogAsync;

				Client.Ready += () =>
				{
					ready = true;
					return Task.CompletedTask;
				};

				await Client.LoginAsync(TokenType.Bot, token);
				await Client.StartAsync();

				while (!ready)
					await Task.Yield();
			}
		}

		public static void Dispose()
		{
			Client?.Dispose();
		}

		private static Task LogAsync(LogMessage log)
		{
			Log.Write(log.ToString(), "Manager/Discord");
			return Task.CompletedTask;
		}
	}
}
