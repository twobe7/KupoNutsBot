// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Quotes
{
	using System;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Quotes;

	public static class QuoteExtensions
	{
		public static Embed ToEmbed(this Quote self)
		{
			if (self.UserId == null)
				throw new Exception("No User Id in quote");

			if (self.GuildId == null)
				throw new Exception("No Guild Id in quote");

			SocketGuild guild = Program.DiscordClient.GetGuild((ulong)self.GuildId);
			SocketGuildUser user = guild.GetUser((ulong)self.UserId);

			EmbedBuilder builder = new EmbedBuilder();

			builder.Author = new EmbedAuthorBuilder();
			builder.Author.Name = user.GetName();
			builder.Author.IconUrl = user.GetAvatarUrl();
			builder.Description = self.Content;
			builder.Timestamp = self.GetDateTime().ToDateTimeOffset();

			return builder.Build();
		}
	}
}
