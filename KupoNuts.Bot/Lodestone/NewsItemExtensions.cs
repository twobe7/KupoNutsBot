// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Lodestone
{
	using System;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using global::Lodestone.News;
	using NodaTime;
	using NodaTime.Text;

	public static class NewsItemExtensions
	{
		public static async Task Post(this NewsItem self, ulong channelID)
		{
			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelID);
			await channel.SendMessageAsync(null, false, self.ToEmbed());
		}

		public static Embed ToEmbed(this NewsItem self)
		{
			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = self.title;
			builder.Url = self.url;
			builder.Description = self.description;
			builder.ImageUrl = self.image;
			builder.Color = self.GetColor();
			builder.Timestamp = self.GetInstant().ToDateTimeOffset();
			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = self.category.ToString();

			return builder.Build();
		}

		public static Instant GetInstant(this NewsItem self)
		{
			return InstantPattern.ExtendedIso.Parse(self.time).Value;
		}

		public static Instant? GetStart(this NewsItem self)
		{
			if (self.start == null)
				return null;

			return InstantPattern.ExtendedIso.Parse(self.start).Value;
		}

		public static Instant? GetEnd(this NewsItem self)
		{
			if (self.end == null)
				return null;

			return InstantPattern.ExtendedIso.Parse(self.end).Value;
		}

		public static Color GetColor(this NewsItem self)
		{
			switch (self.category)
			{
				case Categories.Topics: return Color.DarkGrey;
				case Categories.Notices: return Color.Green;
				case Categories.Maintenance: return Color.Orange;
				case Categories.Updates: return Color.Gold;
				case Categories.Status: return Color.LighterGrey;
				case Categories.Developers: return Color.Teal;
			}

			throw new Exception("Unknown category: " + self.category);
		}
	}
}
