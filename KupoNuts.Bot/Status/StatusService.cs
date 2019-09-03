// This document is intended for use by Kupo Nut Brigade developers.

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

			_ = Task.Factory.StartNew(this.UpdateStatus, TaskCreationOptions.LongRunning);

			await this.PostStatus();
		}

		public override async Task Shutdown()
		{
			this.online = false;
			await this.PostStatus();
		}

		private async Task PostStatus()
		{
			Settings settings = Settings.Load();
			if (settings.StatusChannel == null)
			{
				Log.Write("No Status Channel set. Kupo Nuts will not post logs to discord");
				return;
			}

			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = this.online ? Color.Green : Color.Red;
			builder.Title = "Kupo Nuts Bot Status";

			builder.AddField("Status", this.online ? "Online" : "Offline", true);

			builder.AddField("Last Online", TimeUtils.GetDateTimeString(TimeUtils.Now), true);

			ulong id = ulong.Parse(settings.StatusChannel);
			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(id);

			RestUserMessage message;
			if (settings.StatusMessage == null)
			{
				message = await channel.SendMessageAsync(null, false, builder.Build());
				settings.StatusMessage = message.Id.ToString();
			}
			else
			{
				message = (RestUserMessage)await channel.GetMessageAsync(ulong.Parse(settings.StatusMessage));
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
