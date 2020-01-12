// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;

	public class DiscordService : ServiceBase
	{
		private static DiscordSocketClient client;

		public static DiscordSocketClient DiscordClient
		{
			get
			{
				if (client is null)
					throw new Exception("No Discord client");

				return client;
			}
		}

		public override async Task Initialize()
		{
			await base.Initialize();
			client = new DiscordSocketClient();

			bool ready = false;
			client.Log += this.LogAsync;

			client.Ready += () =>
			{
				ready = true;
				return Task.CompletedTask;
			};

			string token = Settings.Load().Token;
			if (string.IsNullOrEmpty(token))
				throw new Exception("No Token in settings file");

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			while (!ready)
			{
				await Task.Yield();
			}
		}

		public override Task Shutdown()
		{
			client.Dispose();
			return base.Shutdown();
		}

		private Task LogAsync(LogMessage log)
		{
			Log.Write(log.ToString(), "WebServer Bot");
			return Task.CompletedTask;
		}
	}
}
