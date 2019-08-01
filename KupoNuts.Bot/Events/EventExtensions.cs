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

	public static class EventExtensions
	{
		public static async Task Post(this Event self)
		{
			Event.Notification notify = new Event.Notification();
			await notify.Post(self);
			self.Notifications.Add(notify);
			Database.Instance.Save();
		}

		public static async Task UpdateNotifications(this Event self)
		{
			foreach (Event.Notification notify in self.Notifications)
			{
				await notify.Post(self);
			}
		}

		public static async Task CheckReactions(this Event self)
		{
			foreach (Event.Notification notify in self.Notifications)
			{
				await notify.CheckReactions(self);
			}

			await self.UpdateNotifications();
		}

		public static IEmote GetRemindMeEmote(this Event self)
		{
			return Emote.Parse(self.RemindMeEmote);
		}

		public static string GetRepeatsString(this Event self)
		{
			if (self.Repeats == Event.Days.None)
				return null;

			StringBuilder builder = new StringBuilder();
			builder.Append("Every ");

			int count = 0;
			foreach (Event.Days day in Enum.GetValues(typeof(Event.Days)))
			{
				if (FlagsUtils.IsSet(self.Repeats, day))
				{
					if (count > 0)
						builder.Append(", ");

					count++;
					builder.Append(day);
				}
			}

			if (count == 7)
			{
				builder.Clear();
				builder.Append("Every day");
			}

			return builder.ToString();
		}

		public static string GetAttendeeString(this Event self, int statusIndex, out int count)
		{
			StringBuilder builder = new StringBuilder();

			count = 0;
			foreach (Event.Attendee attendee in self.Attendees)
			{
				if (attendee.Status == statusIndex)
				{
					count++;
					builder.AppendLine(attendee.GetMention());
				}
			}

			if (count <= 0)
				builder.Append("No one yet");

			if (count > 8)
			{
				builder.Clear();
				builder.Append(count + " people");
			}

			return builder.ToString();
		}
	}
}
