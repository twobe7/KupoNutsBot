// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
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

			return user.Username;
			////return user.Mention;
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
