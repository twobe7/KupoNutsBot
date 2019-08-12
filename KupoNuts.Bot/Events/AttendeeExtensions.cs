// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Events;
	using NodaTime;
	using NodaTime.Text;

	public static class AttendeeExtensions
	{
		public static bool Is(this Attendee self, ulong userId)
		{
			return self.UserId == userId.ToString();
		}

		public static bool Is(this Attendee self, string userId)
		{
			return self.UserId == userId;
		}

		public static string GetName(this Attendee self)
		{
			ulong userId = ulong.Parse(self.UserId);
			SocketUser user = Program.DiscordClient.GetUser(userId);

			if (user == null)
				return "Unknown";

			Database db = Database.Load();
			Event evt = db.GetEvent(self.EventId);

			SocketGuild guild = Program.DiscordClient.GetGuild(evt.GetServerId());
			if (guild != null)
			{
				SocketGuildUser guildUser = guild.GetUser(userId);
				if (guildUser != null)
				{
					return guildUser.Nickname;
				}
			}

			return user.Mention;
		}

		public static Duration? GetRemindTime(this Attendee self)
		{
			if (string.IsNullOrEmpty(self.RemindTime))
				return null;

			return DurationPattern.Roundtrip.Parse(self.RemindTime).Value;
		}

		public static void SetRemindTime(this Attendee self, Duration? duration)
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
