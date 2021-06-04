// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Lodestone
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
			SocketTextChannel? channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelID);

			if (channel == null)
				return;

			await channel.SendMessageAsync(null, false, self.ToEmbed());
		}

		public static Embed ToEmbed(this NewsItem self)
		{
			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = self.Title;
			builder.Url = self.Url;
			builder.Description = self.Description?.Length > 2048 ? self.Description.Substring(0, 2048) : self.Description;
			builder.ImageUrl = self.Image;
			builder.Color = self.GetColor();
			builder.Timestamp = self.GetInstant().ToDateTimeOffset();
			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = self.Category;

			return builder.Build();
		}

		public static Instant GetInstant(this NewsItem self)
		{
			return InstantPattern.ExtendedIso.Parse(self.Time).Value;
		}

		public static Instant? GetStart(this NewsItem self)
		{
			if (self.Start == null)
				return null;

			return InstantPattern.ExtendedIso.Parse(self.Start).Value;
		}

		public static Instant? GetEnd(this NewsItem self)
		{
			if (self.End == null)
				return null;

			return InstantPattern.ExtendedIso.Parse(self.End).Value;
		}

		public static Color GetColor(this NewsItem self)
		{
			switch (self.Category)
			{
				case "topics": return Color.DarkGrey;
				case "notices": return Color.Green;
				case "maintenance": return Color.Orange;
				case "updates": return Color.Gold;
				case "status": return Color.LighterGrey;
				case "developers": return Color.Teal;
			}

			throw new Exception("Unknown category: " + self.Category);
		}
	}
}
