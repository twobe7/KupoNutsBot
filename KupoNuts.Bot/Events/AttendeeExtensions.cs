// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
	using System;
	using Discord.WebSocket;
	using KupoNuts.Events;
	using NodaTime;
	using NodaTime.Text;

	public static class AttendeeExtensions
	{
		public static bool Is(this Event.Notification.Attendee self, ulong userId)
		{
			if (self.UserId == null)
				return false;

			return ulong.Parse(self.UserId) == userId;
		}

		public static string GetName(this Event.Notification.Attendee self, Event evt)
		{
			if (self.UserId == null)
				throw new ArgumentNullException("Id");

			SocketUser user = Program.DiscordClient.GetUser(ulong.Parse(self.UserId));

			if (user == null)
				return "Unknown";

			SocketGuild guild = Program.DiscordClient.GetGuild(evt.ServerId);
			if (guild != null)
			{
				SocketGuildUser guildUser = guild.GetUser(ulong.Parse(self.UserId));
				if (guildUser != null && !string.IsNullOrEmpty(guildUser.Nickname))
				{
					return guildUser.Nickname;
				}
			}

			return user.Username;
		}

		public static Duration? GetRemindTime(this Event.Notification.Attendee self)
		{
			if (string.IsNullOrEmpty(self.RemindTime))
				return null;

			return DurationPattern.Roundtrip.Parse(self.RemindTime).Value;
		}

		public static void SetRemindTime(this Event.Notification.Attendee self, Duration? duration)
		{
			if (duration == null)
			{
				self.RemindTime = null;
				return;
			}

			self.RemindTime = DurationPattern.Roundtrip.Format((Duration)duration);
		}
	}
}
