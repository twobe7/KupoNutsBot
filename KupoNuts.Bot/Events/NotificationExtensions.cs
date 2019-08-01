// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Utils;

	public static class NotificationExtensions
	{
		public static async Task Post(this Event.Notification self, Event evt)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = evt.Color.ToDiscordColor();
			builder.Title = evt.Name;
			builder.Description = evt.Description;
			builder.ImageUrl = evt.Image;

			/*
			 * Due to a bug on Android, we cannot use the Timestamp field for dates in teh future.
			 * If discord ever fixes this, we should use timestamps as thay automatically adjust to the clients
			 * time zone.
			 * https://trello.com/c/RO4zrt25
			 */
			/*StringBuilder footerBuilder = new StringBuilder();
			footerBuilder.Append(TimeUtils.GetDurationString(evt.Duration));
			footerBuilder.Append(" ");
			footerBuilder.Append(evt.GetRepeatsString());

			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = footerBuilder.ToString();
			builder.Timestamp = evt.NextOccurance();*/

			StringBuilder timeBuilder = new StringBuilder();
			timeBuilder.Append(TimeUtils.GetDurationString(evt.Duration));
			timeBuilder.Append(" ");

			string repeat = evt.GetRepeatsString();
			if (repeat != null)
			{
				timeBuilder.Append(repeat);
				timeBuilder.AppendLine(" at ");
				timeBuilder.Append(TimeUtils.GetTimeString(evt.DateTime));
			}
			else
			{
				timeBuilder.Append(" on ");
				timeBuilder.Append(TimeUtils.GetDateTimeString(evt.DateTime));
			}

			builder.AddField("When", timeBuilder.ToString(), false);

			for (int i = 0; i < evt.Statuses.Count; i++)
			{
				Event.Status status = evt.Statuses[i];

				if (string.IsNullOrEmpty(status.Display))
					continue;

				int count = 0;
				string attending = evt.GetAttendeeString(i, out count);

				builder.AddField(status.Display + " (" + count + ")", attending, true);
			}

			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(evt.ChannelId);

			RestUserMessage message;
			if (self.MessageId == 0)
			{
				message = await channel.SendMessageAsync(null, false, builder.Build());
				self.MessageId = message.Id;

				List<IEmote> reactions = new List<IEmote>();

				if (!string.IsNullOrEmpty(evt.RemindMeEmote))
					reactions.Add(evt.GetRemindMeEmote());

				foreach (Event.Status status in evt.Statuses)
				{
					reactions.Add(status.GetEmote());
				}

				await message.AddReactionsAsync(reactions.ToArray());

				EventsService.Instance.Watch(self.MessageId, evt);
			}
			else
			{
				message = (RestUserMessage)await channel.GetMessageAsync(self.MessageId);
				await message.ModifyAsync(x =>
				{
					x.Embed = builder.Build();
				});
			}
		}

		public static async Task CheckReactions(this Event.Notification self, Event evt)
		{
			if (self.MessageId == 0)
				return;

			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(evt.ChannelId);
			RestUserMessage message = (RestUserMessage)await channel.GetMessageAsync(self.MessageId);

			for (int i = 0; i < evt.Statuses.Count; i++)
			{
				Event.Status status = evt.Statuses[i];
				await self.CheckReactions(evt, channel, message, status, i);
			}
		}

		private static async Task CheckReactions(this Event.Notification self, Event evt, SocketTextChannel channel, RestUserMessage message, Event.Status status, int statusIndex)
		{
			IEmote emote = status.GetEmote();
			IEnumerable<IUser> checkedUsers = await message.GetReactionUsersAsync(emote, 99).FlattenAsync();

			if (!checkedUsers.Contains(Program.DiscordClient.CurrentUser))
				await message.AddReactionAsync(emote);

			foreach (IUser user in checkedUsers)
			{
				if (user.Id == Program.DiscordClient.CurrentUser.Id)
					continue;

				Event.Attendee attendee = evt.GetAttendee(user.Id);
				attendee.Status = statusIndex;

				SocketUser socketUser = Program.DiscordClient.GetUser(user.Id);
				await message.RemoveReactionAsync(emote, socketUser);
			}
		}

		private static bool Contains(this IEnumerable<IUser> self, IUser item)
		{
			foreach (IUser user in self)
			{
				if (user.Id == item.Id)
				{
					return true;
				}
			}

			return false;
		}
	}
}
