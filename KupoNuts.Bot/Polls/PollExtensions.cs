// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Polls
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;

	public static class PollExtensions
	{
		public static async Task<bool> IsValid(this Poll self)
		{
			SocketTextChannel? channel = self.GetChannel();
			if (channel is null)
				return false;

			if (self.Options != null)
			{
				foreach (string option in self.Options)
				{
					if (!ulong.TryParse(option, out ulong messageID))
						return false;

					IMessage optionMessage = await channel.GetMessageAsync(messageID);

					if (optionMessage == null)
					{
						return false;
					}
				}
			}

			return true;
		}

		public static SocketTextChannel? GetChannel(this Poll self)
		{
			if (string.IsNullOrEmpty(self.ChannelId))
				return null;

			ulong id = ulong.Parse(self.ChannelId);

			SocketChannel channel = Program.DiscordClient.GetChannel(id);

			if (channel is SocketTextChannel textChannel)
				return textChannel;

			throw new Exception("Channel: \"" + self.ChannelId + "\" is not a text channel");
		}
	}
}
