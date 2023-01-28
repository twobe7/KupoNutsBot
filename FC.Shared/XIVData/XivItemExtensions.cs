// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	using System.Text;
	using Discord;

	public static class XivItemExtensions
	{
		public static EmbedBuilder ToMbEmbed(this XivItem self)
		{
			EmbedBuilder builder = new EmbedBuilder()
			{
				Title = self.Name,
				ThumbnailUrl = self.IconFullPath,
				Color = Color.Teal,
			};

			StringBuilder desc = new StringBuilder();

			// universalis link
			desc.AppendLine($"[Universalis](https://universalis.app/market/{self.Id}) (ID: {self.Id})");

			builder.Description = desc.ToString();

			return builder;
		}
	}
}
