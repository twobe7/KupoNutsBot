// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
	using Discord.WebSocket;
	using KupoNuts.Events;
	using NodaTime;
	using NodaTime.Text;

	public static class AttendeeExtensions
	{
		public static string GetMention(this Event.Attendee self)
		{
			SocketUser user = Program.DiscordClient.GetUser(self.UserId);
			return user.Mention;
		}

		public static Duration? GetRemindTime(this Event.Attendee self)
		{
			if (string.IsNullOrEmpty(self.RemindTime))
				return null;

			return DurationPattern.Roundtrip.Parse(self.RemindTime).Value;
		}

		public static void SetRemindTime(this Event.Attendee self, Duration? duration)
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
