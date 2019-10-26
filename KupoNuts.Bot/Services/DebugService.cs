// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Utils;
	using NodaTime;

	public class DebugService : ServiceBase
	{
		private static readonly List<string> GoodBotResponses = new List<string>()
		{
			"Thanks!",
			"Thanks, you're a good human!",
			"Sure, I guess...",
			"yaay!",
			"I try so hard!",
			"It's nice to get some recognition around here.",
		};

		public override Task Initialize()
		{
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			return Task.CompletedTask;
		}

		[Command("GoodBot", Permissions.Everyone, "Ye")]
		public Task<string> GoodBot()
		{
			Random rn = new Random();
			return Task.FromResult(GoodBotResponses[rn.Next(0, GoodBotResponses.Count)]);
		}

		[Command("Time", Permissions.Everyone, "Shows the current time in multiple time zones.")]
		public Task<string> Time()
		{
			Instant now = SystemClock.Instance.GetCurrentInstant();
			return Task.FromResult("The time is: " + TimeUtils.GetDateTimeString(now));
		}

		[Command("Time", Permissions.Everyone, "Shows the current time in the given timezone")]
		public Task<string> Time(string timezone)
		{
			DateTimeZone dtz;

			try
			{
				dtz = TimeUtils.GetTimeZone(timezone);
			}
			catch (Exception)
			{
				throw new UserException("I couldn't find that timezone! Try \"Australia/Sydney\"");
			}

			Instant now = SystemClock.Instance.GetCurrentInstant();
			return Task.FromResult("The time is: " + TimeUtils.GetDateTimeString(now, dtz));
		}

		[Command("Blame", Permissions.Everyone, "Blames someone")]
		public Task<string> Blame(CommandMessage message)
		{
			if (message.Channel is SocketGuildChannel guildChannel)
			{
				List<IGuildUser> targets = new List<IGuildUser>();
				targets.Add(message.Author);

				foreach (SocketGuildUser tTarget in guildChannel.Guild.Users)
				{
					if (CommandsService.GetPermissions(tTarget) != Permissions.Administrators)
						continue;

					targets.Add(tTarget);
				}

				Random rnd = new Random();
				int val = rnd.Next(targets.Count);
				IGuildUser target = targets[val];

				if (target.Id == Program.DiscordClient.CurrentUser.Id)
					return Task.FromResult("This is my fault. =(");

				return Task.FromResult("This is your fault, " + target.GetName() + ".");
			}

			return Task.FromResult("This is your fault, " + message.Author.GetName() + ".");
		}

		[Command("Confus", Permissions.Administrators, "??")]
		public async Task<bool> Confus(CommandMessage message)
		{
			await message.Channel.SendFileAsync(PathUtils.Current + "/Assets/confus.jpg");
			return true;
		}
	}
}
