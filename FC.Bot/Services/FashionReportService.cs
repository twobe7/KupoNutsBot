// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Data;
	using Twitter;

	public class FashionReportService : ServiceBase
	{
		private Table<FashionReportEntry> db = new Table<FashionReportEntry>("KupoNuts_FashionReport", 0);

		public override async Task Initialize()
		{
			await base.Initialize();

			await this.db.Connect();

			await this.Update();
			ScheduleService.RunOnSchedule(this.Update, 60);
		}

		[Command("FashionReport", Permissions.Everyone, "Gets the latest Fashion Report post", CommandCategory.News)]
		[Command("fr", Permissions.Everyone, "Gets the latest Fashion Report post", CommandCategory.News, "FashionReport")]
		public async Task<Embed> GetFashionReport()
		{
			List<FashionReportEntry> reports = await FashionReportAPI.Get();
			reports.Sort((a, b) =>
			{
				return a.Time.CompareTo(b.Time);
			});

			foreach (FashionReportEntry entry in reports)
			{
				if (entry.Id == null)
					continue;

				return this.GetEmbed(entry);
			}

			throw new UserException("I couldn't find any Fashion Report posts.");
		}

		private async Task Update()
		{
			List<FashionReportEntry> reports = await FashionReportAPI.Get();
			foreach (FashionReportEntry entry in reports)
			{
				if (entry.Id == null)
					continue;

				FashionReportEntry? saved = await this.db.Load(entry.Id);
				if (saved == null)
				{
					await this.Post(entry);
					await this.db.Save(entry);
				}
			}
		}

		private async Task Post(FashionReportEntry entry)
		{
			Log.Write("Posting Fashion Report: " + entry.Content, "Bot");

			foreach (SocketGuild guild in Program.DiscordClient.Guilds)
			{
				GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guild.Id);

				if (settings.FashionReportChannel == null)
					continue;

				ulong channelId = ulong.Parse(settings.FashionReportChannel);
				SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);

				if (channel == null)
					continue;

				await channel.SendMessageAsync(null, false, this.GetEmbed(entry));
			}
		}

		private Embed GetEmbed(FashionReportEntry entry)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Author = new EmbedAuthorBuilder();
			builder.Author.IconUrl = entry.AuthorImageUrl;
			builder.Author.Name = entry.Author;
			builder.ImageUrl = entry.ImageUrl;
			builder.Description = entry.Content;
			builder.Color = Color.Magenta;
			builder.Timestamp = entry.Time;
			return builder.Build();
		}
	}
}
