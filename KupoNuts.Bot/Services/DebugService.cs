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
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			return Task.CompletedTask;
		}

		[Command("Time", Permissions.Everyone, "Shows the current time in multiple time zones.")]
		public Task<string> Time()
		{
			Instant now = SystemClock.Instance.GetCurrentInstant();
			return Task.FromResult("The time is: " + TimeUtils.GetDateTimeString(now));
		}
	}
}
