// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Events;
	using KupoNuts.Bot.Services;
	using KupoNuts.Bot.Status;

	public class Program
	{
		private static List<ServiceBase> services = new List<ServiceBase>();
		private static bool exiting = false;

		public static DiscordSocketClient DiscordClient
		{
			get;
			private set;
		}

		public static bool Running
		{
			get;
			private set;
		}

		public static Task Run(string[] args)
		{
			Program prog = new Program();
			return prog.DoRun();
		}

		public static async Task Exit()
		{
			exiting = true;

			while (Running)
			{
				await Task.Yield();
			}
		}

		protected virtual async Task AddServices()
		{
			try
			{
				await this.AddService<CommandsService>();
				await this.AddService<DebugService>();
				await this.AddService<StatusService>();
				await this.AddService<EventsService>();
				await this.AddService<ReminderService>();
				await this.AddService<EchoService>();
				await this.AddService<KarmaService>();
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task DoRun()
		{
			try
			{
				Running = true;
				Log.Write("Kupo Nuts Bot booting..");

				if (string.IsNullOrEmpty(Database.Load().Token))
				{
					Log.Write("No token set. Please set a token in the Database file");
				}
				else
				{
					DiscordClient = new DiscordSocketClient();

					bool ready = false;
					DiscordClient.Log += this.LogAsync;

					DiscordClient.Ready += () =>
					{
						ready = true;
						return Task.CompletedTask;
					};

					await DiscordClient.LoginAsync(TokenType.Bot, Database.Load().Token);
					await DiscordClient.StartAsync();

					while (!ready)
						await Task.Yield();

					await this.AddServices();

					while (!exiting)
					{
						await Task.Yield();
						Thread.Sleep(100);
					}
				}

				Log.Write("shutting down");

				if (services != null)
				{
					foreach (ServiceBase service in services)
					{
						await service.Shutdown();
					}
				}

				DiscordClient?.Dispose();
				Running = false;
				Log.Write("shutdown complete");
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task AddService<T>()
			where T : ServiceBase
		{
			T service = Activator.CreateInstance<T>();
			await service.Initialize();
			services.Add(service);
		}

		private Task LogAsync(LogMessage log)
		{
			Log.Write(log.ToString());
			return Task.CompletedTask;
		}
	}
}
