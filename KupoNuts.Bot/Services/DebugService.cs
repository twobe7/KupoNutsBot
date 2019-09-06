// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
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

		[Command("Blame", Permissions.Everyone, "Blames someone")]
		public Task<string> Blame(SocketMessage message)
		{
			if (message.Channel is SocketGuildChannel guildChannel)
			{
				List<SocketGuildUser> targets = new List<SocketGuildUser>();

				foreach (SocketGuildUser tTarget in guildChannel.Guild.Users)
				{
					if (CommandsService.GetPermissions(tTarget) != Permissions.Administrators)
						continue;

					targets.Add(tTarget);
				}

				if (targets.Count <= 0)
					throw new Exception("No administrators to blame!");

				Random rnd = new Random();
				int val = rnd.Next(targets.Count);
				SocketGuildUser target = targets[val];

				if (target.Id == Program.DiscordClient.CurrentUser.Id)
					return Task.FromResult("This is my failt.\n>>BadBot");

				return Task.FromResult("This is your fault, " + target.Mention);
			}

			return Task.FromResult("This is your fault, " + message.Author.Mention);
		}
	}
}
