// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System.Linq;
	using System.Threading.Tasks;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Data;
	using Twitter;

	[Group("fashion-report", "For Streamers")]
	public class FashionReportService(DiscordSocketClient discordClient) : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient = discordClient;

		private static readonly Table<FashionReportEntry> FashionReportDatabase = new("KupoNuts_FashionReport", 0);

		public override async Task Initialize()
		{
			await base.Initialize();

			await FashionReportDatabase.Connect();

			await this.Update();
			ScheduleService.RunOnSchedule(this.Update, 60);
		}

		[SlashCommand("latest", "Get the latest fashion report post by KaiyokoStar")]
		public async Task GetFashionReport()
		{
			await this.DeferAsync();

			var fashionReportEntry = await FashionReportDatabase.LoadAll(null);
			var latest = fashionReportEntry.OrderByDescending(x => x.Time).FirstOrDefault();

			if (latest != null)
			{
				await this.FollowupAsync(embeds: [latest.GetEmbed()]);
				return;
			}

			await this.FollowupAsync("I couldn't find any Fashion Report posts.");
		}

		private async Task Update()
		{
			var fashionReportEntry = await FashionReportDatabase.LoadAll(null);
			var latest = fashionReportEntry.OrderByDescending(x => x.Time).FirstOrDefault();

			var lastPostId = latest?.Id;

			FashionReportEntry? entry = await FashionReportAPI.GetLatest(lastPostId);

			if (entry == null)
				return;

			FashionReportEntry? saved = await FashionReportDatabase.Load(entry.Id);
			if (saved == null)
			{
				await this.Post(entry);
				await FashionReportDatabase.Save(entry);
			}
		}

		private async Task Post(FashionReportEntry entry)
		{
			Log.Write($"Posting Fashion Report: {entry.Content}", "Bot");

			foreach (SocketGuild guild in this.DiscordClient.Guilds)
			{
				GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guild.Id);

				if (settings.FashionReportChannel == null)
					continue;

				ulong channelId = ulong.Parse(settings.FashionReportChannel);
				SocketTextChannel channel = (SocketTextChannel)this.DiscordClient.GetChannel(channelId);

				if (channel == null)
					continue;

				await channel.SendMessageAsync(null, false, entry.GetEmbed());
			}
		}
	}
}
