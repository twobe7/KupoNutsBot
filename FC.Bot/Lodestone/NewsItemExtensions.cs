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
			EmbedBuilder builder = new EmbedBuilder
			{
				Title = self.Title,
				Url = self.Url,
				Description = self.Description?.Length > 2048 ? self.Description.Substring(0, 2048) : self.Description,
				ImageUrl = self.Image,
				Color = self.GetColor(),
				Timestamp = self.GetInstant().ToDateTimeOffset(),
				Footer = new EmbedFooterBuilder
				{
					Text = self.Category,
				},
			};

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
			return self.Category switch
			{
				"topics" => Color.DarkGrey,
				"notices" => Color.Green,
				"maintenance" => Color.Orange,
				"updates" => Color.Gold,
				"status" => Color.LighterGrey,
				"developers" => Color.Teal,
				_ => throw new Exception("Unknown category: " + self.Category),
			};
		}
	}
}
