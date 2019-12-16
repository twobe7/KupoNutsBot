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

			ScheduleService.RunOnSchedule(this.UpdateStatus, 15);

			await this.UpdateStatus();
		}

		public override async Task Shutdown()
		{
			this.online = false;
			await this.UpdateStatus();
		}

		private async Task UpdateStatus()
		{
			Settings settings = Settings.Load();
			if (settings.StatusChannel == null)
			{
				Log.Write("No Status Channel set. Kupo Nuts will not post logs to discord", "Bot");
				return;
			}

			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = this.online ? Color.Green : Color.Red;
			builder.Title = "Kupo Nuts Bot Status";

			builder.AddField("Status", this.online ? "Online" : "Offline", true);

			builder.AddField("Last Online", TimeUtils.GetDateTimeString(TimeUtils.Now), true);

			ulong id = ulong.Parse(settings.StatusChannel);
			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(id);

			RestUserMessage? message = null;

			if (settings.StatusMessage != null)
				message = (RestUserMessage)await channel.GetMessageAsync(ulong.Parse(settings.StatusMessage));

			if (message == null)
			{
				message = await channel.SendMessageAsync(null, false, builder.Build());
				settings.StatusMessage = message.Id.ToString();
				settings.Save();
			}
			else
			{
				await message.ModifyAsync(x =>
				{
					x.Embed = builder.Build();
				});
			}
		}
	}
}
