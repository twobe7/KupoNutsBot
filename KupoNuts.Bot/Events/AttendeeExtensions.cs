// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using Discord.WebSocket;

	public static class AttendeeExtensions
	{
		public static string GetMention(this Event.Attendee self)
		{
			SocketUser user = Program.DiscordClient.GetUser(self.UserId);
			return user.Mention;
		}
	}
}
