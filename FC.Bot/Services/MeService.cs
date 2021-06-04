// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;

	public class MeService : ServiceBase
	{
		public override async Task Initialize()
		{
			await base.Initialize();

			ScheduleService.RunOnSchedule(this.Update);
			await this.Update();
		}

		private async Task Update()
		{
			foreach (SocketGuild guild in Program.DiscordClient.Guilds)
			{
				GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guild.Id);

				string? name = null;
				if (!string.IsNullOrEmpty(settings.NickName))
					name = settings.NickName;

				SocketGuildUser guildUser = guild.GetUser(Program.DiscordClient.CurrentUser.Id);

				if (guildUser != null)
				{
					await guildUser.ModifyAsync(x =>
					{
						x.Nickname = name;
					});

					await Task.Delay(500); // don't flood
				}
			}

			await Program.DiscordClient.SetActivityAsync(new BotActivity());
		}

		private class BotActivity : IActivity
		{
			public string Name
			{
				get
				{
					return "over " + Program.DiscordClient.Guilds.Count + " guilds";
				}
			}

			public ActivityType Type
			{
				get
				{
					return ActivityType.Watching;
				}
			}

			public string Details
			{
				get { return "KUPO NUTS"; }
			}

			public ActivityProperties Flags
			{
				get { return ActivityProperties.None; }
			}
		}
	}
}
