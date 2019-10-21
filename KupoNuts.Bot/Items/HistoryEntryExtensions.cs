// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Items
{
	using System.Text;
	using Universalis;

	public static class HistoryEntryExtensions
	{
		public static string ToStringEx(this HistoryAPI.Entry self)
		{
			StringBuilder builder = new StringBuilder();
			if (self.hq == true)
			{
				builder.Append(ItemService.HighQualityEmote);
			}
			else
			{
				builder.Append(ItemService.NormalQualityEmote);
			}

			builder.Append(self.pricePerUnit?.ToString("N0"));
			builder.Append("g - ");
			builder.Append(self.worldName);
			builder.Append(" ");

			return builder.ToString();
		}
	}
}
