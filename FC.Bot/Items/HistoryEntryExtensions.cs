// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Items
{
	using System.Text;
	using Universalis;

	public static class HistoryEntryExtensions
	{
		public static string ToStringEx(this MarketAPI.History self)
		{
			StringBuilder builder = new StringBuilder();
			if (self.Hq == true)
			{
				builder.Append(ItemService.HighQualityEmote);
			}
			else
			{
				builder.Append(ItemService.NormalQualityEmote);
			}

			builder.Append(self.PricePerUnit?.ToString("N0"));
			builder.Append("g - ");
			builder.Append(self.WorldName);
			builder.Append(" ");

			return builder.ToString();
		}
	}
}
