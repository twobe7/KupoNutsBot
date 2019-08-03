// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Utils;
	using NodaTime;

	public class DebugService : ServiceBase
	{
		public override Task Initialize()
		{
			CommandsService.BindCommand("time", this.Time, Permissions.Everyone, "Shows the current time in multiple time zones.");
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("time");
			return Task.CompletedTask;
		}

		private async Task Time(string[] args, SocketMessage message)
		{
			Instant now = SystemClock.Instance.GetCurrentInstant();
			await message.Channel.SendMessageAsync("The time is: " + TimeUtils.GetDateTimeString(now));
		}
	}
}
