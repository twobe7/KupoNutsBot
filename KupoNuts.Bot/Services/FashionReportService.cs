// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using Twitter;

	public class FashionReportService : ServiceBase
	{
		private Database<FashionReportEntry> db = new Database<FashionReportEntry>("FashionReport", 0);

		public override async Task Initialize()
		{
			await base.Initialize();

			await this.db.Connect();

			await this.Update();
			Scheduler.RunOnSchedule(this.Update, 60);
		}

		[Command("FashionReport", Permissions.Everyone, "Gets the latest Fashion Report post")]
		[Command("fr", Permissions.Everyone, "Gets the latest Fashion Report post")]
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

			string? channelIdStr = Settings.Load().FashionReportChannel;
			if (channelIdStr == null)
				return;

			ulong channelId = ulong.Parse(channelIdStr);
			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);

			await channel.SendMessageAsync(null, false, this.GetEmbed(entry));
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
