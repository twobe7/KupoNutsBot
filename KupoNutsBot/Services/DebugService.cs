// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using KupoNutsBot.Commands;
	using KupoNutsBot.Utils;
	using NodaTime;

	public class DebugService : ServiceBase
	{
		public override Task Initialize()
		{
			CommandsService.BindCommand("time", this.Time, Permissions.Everyone);
			CommandsService.BindCommand("notImplemented", this.Test, Permissions.Administrators);
			CommandsService.BindCommand("error", this.Error, Permissions.Administrators);
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("time");
			CommandsService.ClearCommand("notImplemented");
			CommandsService.ClearCommand("error");
			return Task.CompletedTask;
		}

		private async Task Time(string[] args, SocketMessage message)
		{
			Instant now = SystemClock.Instance.GetCurrentInstant();
			await message.Channel.SendMessageAsync("The time is: " + TimeUtils.GetDateTimeString(now));
		}

		private Task Test(string[] args, SocketMessage message)
		{
			throw new NotImplementedException();
		}

		private Task Error(string[] args, SocketMessage message)
		{
			throw new Exception("This is a test error. Nothing has gone wrong.");
		}
	}
}
