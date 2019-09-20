// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Items
{
	using System.Text;
	using Discord;
	using Universalis;

	public static class HistoryEntryExtensions
	{
		public static void ToEmbed(this HistoryAPI.Entry self, EmbedBuilder embed)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(ItemService.GilEmote);
			builder.Append(self.pricePerUnit?.ToString("N0"));
			builder.Append("g - ");
			builder.Append(self.worldName);

			if (self.hq == true)
			{
				builder.Append("  ");
				builder.Append(ItemService.HighQualityEmote);
			}

			embed.AddField("Best Marketboard Price", builder.ToString());
		}
	}
}
