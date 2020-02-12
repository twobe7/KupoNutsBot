// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Actions
{
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using Discord;
	using XIVAPI;

	public static class ActionExtensions
	{
		public static EmbedBuilder ToEmbed(this Action self)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = self.Name;
			builder.ThumbnailUrl = Icons.GetIconURL(self.Icon);

			StringBuilder desc = new StringBuilder();

			desc.AppendFormat("**Level {0} {1}**", self.ClassJobLevel, self.ClassJobCategory?.Name);
			desc.AppendLine();

			if (!string.IsNullOrEmpty(self.Description))
			{
				desc.AppendLine(self.GetDescription());
				desc.AppendLine();
			}

			/* TODO: Append additional information, MP Cost, Cast times, etc. */

			// Garland tools link
			desc.Append("[Garland Tools Database](");
			desc.Append("http://www.garlandtools.org/db/#action/");
			desc.Append(self.ID);
			desc.AppendLine(")");

			// gamer escape link
			desc.Append("[Gamer Escape](");
			desc.Append("https://ffxiv.gamerescape.com/wiki/Special:Search/");
			desc.Append(self.Name.Replace(" ", "%20"));
			desc.AppendLine(")");

			StringBuilder footerText = new StringBuilder();
			footerText.Append("ID: ");
			footerText.Append(self.ID.ToString());
			footerText.Append(" - XIVAPI.com");

			builder.Description = desc.ToString();
			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = footerText.ToString();
			builder.Color = Color.Teal;

			return builder;
		}

		public static string GetDescription(this Action self)
		{
			if (self.Description == null)
				return string.Empty;

			string desc = self.Description;
			desc = Regex.Replace(self.Description, "<.*?>", string.Empty);

			while (desc.Contains("\n\n"))
				desc = desc.Replace("\n\n", "\n");

			return desc;
		}
	}
}
