// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Extensions;
	using FC.Eventsv2;
	using FC.Utils;
	using NodaTime;

	public class Notice
	{
		public readonly Event.Rule Rule;
		public readonly Event Event;
		public readonly Event.Notice EventNotice;

		public Notice(Event evt, Event.Notice notice, Event.Rule rule)
		{
			this.Event = evt;
			this.EventNotice = notice;
			this.Rule = rule;
		}

		public async Task Update()
		{
			if (this.Event == null)
				throw new Exception("Notice has no owner");

			SocketTextChannel? channel = Program.DiscordClient.GetChannel(this.Event.Channel) as SocketTextChannel;
			if (channel is null)
				return;

			EmbedBuilder builder = await this.BuildEmbed();

			RestUserMessage? message = null;
			if (this.EventNotice.MessageId != null)
				message = await channel.GetMessageAsync((ulong)this.EventNotice.MessageId) as RestUserMessage;

			if (message is null)
			{
				message = await channel.SendMessageAsync(null, false, builder.Build());
				this.EventNotice.MessageId = message.Id;
				await EventsService.SaveEvent(this.Event);
			}
			else
			{
				await message.ModifyAsync(x =>
				{
					x.Embed = builder.Build();
				});
			}
		}

		public override int GetHashCode()
		{
			return this.EventNotice.Start.GetHashCode();
		}

		private Task<EmbedBuilder> BuildEmbed()
		{
			if (this.Event is null)
				throw new Exception("Notice has no owner");

			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = this.Event.Name;
			builder.Description = this.Event.Description;
			builder.ImageUrl = this.Event.ImageUrl;

			builder.AddField("Time", this.GetStartEndString());
			builder.AddField("Repeats", this.Event.GetRepeatString());

			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = "Timezone: " + this.Event.BaseTimeZone?.ToString() + " (UTC" + this.Event.BaseTimeZone?.GetUtcOffset(TimeUtils.Now) + ")";

			return Task.FromResult(builder);
		}

		// 19:00 to 22:00
		private string GetStartEndString()
		{
			if (this.Rule == null)
				throw new Exception("Notice has no rule");

			if (this.Event is null)
				throw new Exception("Notice has no owner");

			LocalTime start = this.Event.GetLocalTime(this.EventNotice.Start);
			LocalTime end = this.Event.GetLocalTime(this.EventNotice.Start.Plus(this.Rule.Duration));

			StringBuilder builder = new StringBuilder();
			builder.Append(start.ToDisplayString());
			builder.Append(" to ");
			builder.Append(end.ToDisplayString());
			return builder.ToString();
		}
	}
}
