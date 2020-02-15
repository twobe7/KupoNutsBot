// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Events
{
	using System;
	using Discord.WebSocket;
	using FC.Bot.Extensions;
	using FC.Events;
	using NodaTime;
	using NodaTime.Text;

	public static class AttendeeExtensions
	{
		public static bool Is(this Event.Instance.Attendee self, ulong userId)
		{
			if (self.UserId == null)
				return false;

			return ulong.Parse(self.UserId) == userId;
		}

		public static string GetName(this Event.Instance.Attendee self, Event evt)
		{
			if (self.UserId == null)
				throw new ArgumentNullException("Id");

			SocketUser user = Program.DiscordClient.GetUser(ulong.Parse(self.UserId));

			if (user == null)
				return "Unknown";

			SocketGuild guild = Program.DiscordClient.GetGuild(evt.ServerIdStr);
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

		public static string GetMention(this Event.Instance.Attendee self, Event evt)
		{
			if (self.UserId == null)
				throw new ArgumentNullException("Id");

			SocketUser user = Program.DiscordClient.GetUser(ulong.Parse(self.UserId));

			if (user == null)
				return "Unknown";

			SocketGuild guild = Program.DiscordClient.GetGuild(evt.ServerIdStr);
			if (guild != null)
			{
				SocketGuildUser guildUser = guild.GetUser(ulong.Parse(self.UserId));
				if (guildUser != null)
				{
					return guildUser.Mention;
				}
			}

			return user.Username;
		}
	}
}
