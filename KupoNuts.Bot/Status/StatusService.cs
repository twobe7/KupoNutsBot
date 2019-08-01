﻿// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Status
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using KupoNuts.Utils;

	public class StatusService : ServiceBase
	{
		private bool online;

		public override async Task Initialize()
		{
			this.online = true;

			CommandsService.BindCommand("SetStatusChannel", this.SetStatusChannel, Permissions.Administrators, "Sets the channel for posting status.");

			_ = Task.Factory.StartNew(() => this.UpdateStatus(), TaskCreationOptions.LongRunning);

			await this.PostStatus();
		}

		public override async Task Shutdown()
		{
			CommandsService.ClearCommand("SetStatusChannel");

			this.online = false;
			await this.PostStatus();
		}

		private async Task SetStatusChannel(string[] args, SocketMessage message)
		{
			Database db = Database.Load();

			if (db.StatusChannel == message.Channel.Id)
				return;

			db.StatusChannel = message.Channel.Id;
			db.StatusMessage = 0;
			db.Save();

			await message.Channel.SendMessageAsync("Got it. I'll post the status to this chanel");

			await this.PostStatus();
		}

		private async Task PostStatus()
		{
			Database db = Database.Load();

			if (db.LogChannel <= 0)
			{
				Log.Write("No Status Channel set. Kupo Nuts will not post logs to discord");
				return;
			}

			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = this.online ? Color.Green : Color.Red;
			builder.Title = "Kupo Nuts Bot Status";

			builder.AddField("Status", this.online ? "Online" : "Offline", true);

			builder.AddField("Last Online", TimeUtils.GetDateTimeString(TimeUtils.Now), true);

			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(db.LogChannel);

			RestUserMessage message;
			if (db.StatusMessage == 0)
			{
				message = await channel.SendMessageAsync(null, false, builder.Build());

				db = Database.Load();
				db.StatusMessage = message.Id;
				db.Save();
			}
			else
			{
				message = (RestUserMessage)await channel.GetMessageAsync(db.StatusMessage);
				await message.ModifyAsync(x =>
				{
					x.Embed = builder.Build();
				});
			}
		}

		private async Task UpdateStatus()
		{
			while (this.online)
			{
				int minutes = DateTime.UtcNow.Minute;
				int delay = 15 - minutes;
				while (delay < 0)
					delay += 15;

				await Task.Delay(new TimeSpan(0, delay, 0));

				try
				{
					Log.Write("Updating status");
					await this.PostStatus();
				}
				catch (Exception ex)
				{
					Log.Write(ex);
				}

				await Task.Delay(new TimeSpan(0, 2, 0));
			}
		}
	}
}
