// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
	using Discord.WebSocket;
	using KupoNuts.Events;

	public static class AttendeeExtensions
	{
		public static string GetMention(this Event.Attendee self)
		{
			SocketUser user = Program.DiscordClient.GetUser(self.UserId);
			return user.Mention;
		}
	}
}
