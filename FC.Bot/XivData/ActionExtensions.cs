// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.XivData;

using System.Text;
using System.Text.RegularExpressions;
using Discord;
using XIVAPI;

public static class ActionExtensions
{
	public static EmbedBuilder ToEmbed(this Action self)
	{
		EmbedBuilder builder = new EmbedBuilder()
			.WithTitle(self.Name)
			.WithThumbnailUrl(Icons.GetIconURL(self.Icon));

		StringBuilder desc = new ();

		desc.AppendLine($"**Level {self.ClassJobLevel} {self.ClassJobCategory?.Name}**");

		if (!string.IsNullOrEmpty(self.Description))
		{
			desc.AppendLine(self.GetDescription());
			desc.AppendLine();
		}

		/* TODO: Append additional information, MP Cost, Cast times, etc. */

		// Garland tools link
		desc.Append($"[Garland Tools Database](http://www.garlandtools.org/db/#action/{self.ID})");

		// gamer escape link
		desc.Append($"[Gamer Escape](https://ffxiv.gamerescape.com/wiki/Special:Search/{self.Name.Replace(" ", "%20")})");

		builder.Description = desc.ToString();
		builder.Footer = new EmbedFooterBuilder().WithText($"ID: {self.ID} - XIVAPI.com");
		builder.Color = Color.Teal;

		return builder;
	}

	public static string GetDescription(this Action self)
	{
		if (self.Description == null)
			return string.Empty;

		string desc = Regex.Replace(self.Description, "<.*?>", string.Empty);

		while (desc.Contains("\n\n"))
			desc = desc.Replace("\n\n", "\n");

		return desc;
	}
}