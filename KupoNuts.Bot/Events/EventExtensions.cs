// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Events;
	using KupoNuts.Utils;
	using NodaTime;
	using NodaTime.Text;

	public static class EventExtensions
	{
		public static async Task CheckReactions(this Event self)
		{
			if (self.Notify == null)
				return;

			await self.Notify.CheckReactions(self);
		}

		public static IEmote GetRemindMeEmote(this Event self)
		{
			return Emote.Parse(self.RemindMeEmote);
		}

		public static SocketTextChannel? GetChannel(this Event self)
		{
			if (string.IsNullOrEmpty(self.ChannelId))
				return null;

			ulong id = ulong.Parse(self.ChannelId);

			SocketChannel channel = Program.DiscordClient.GetChannel(id);

			if (channel is SocketTextChannel textChannel)
				return textChannel;

			throw new Exception("Channel: \"" + self.ChannelId + "\" is not a text channel");
		}

		public static void SetAttendeeStatus(this Event self, ulong userId, int status)
		{
			Event.Notification.Attendee? attendee = self.GetAttendee(userId);

			if (attendee == null)
			{
				attendee = new Event.Notification.Attendee();
				attendee.UserId = userId.ToString();
			}

			attendee.Status = status;
		}

		public static List<Event.Notification.Attendee>? GetAttendees(this Event self)
		{
			if (self.Notify == null)
				return null;

			return self.Notify.Attendees;
		}

		public static Event.Notification.Attendee? GetAttendee(this Event self, ulong userId)
		{
			if (self.Id == null)
				throw new ArgumentNullException("Id");

			if (self.Notify == null)
				throw new Exception("Attempt to get attendees for event without an active notification");

			foreach (Event.Notification.Attendee attendee in self.Notify.Attendees)
			{
				if (!attendee.Is(userId))
					continue;

				return attendee;
			}

			return null;
		}

		public static string GetAttendeeString(this Event self, int statusIndex, out int total)
		{
			if (self.Notify == null)
				throw new Exception("Attempt to get attendee string without event notification");

			StringBuilder builder = new StringBuilder();

			total = 0;
			foreach (Event.Notification.Attendee attendee in self.Notify.Attendees)
			{
				if (attendee.Status == statusIndex)
				{
					total++;
				}
			}

			int count = 0;
			foreach (Event.Notification.Attendee attendee in self.Notify.Attendees)
			{
				if (attendee.Status == statusIndex)
				{
					count++;

					if (total > 8)
					{
						if (count > 1)
							builder.Append(", ");

						builder.Append(attendee.GetName(self));
					}
					else
					{
						builder.AppendLine(attendee.GetName(self));
					}
				}
			}

			if (total <= 0)
				builder.Append("No one yet");

			return builder.ToString();
		}
	}
}
