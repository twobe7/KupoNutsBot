// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.XivData;

using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Discord;
using FFXIVCollect;

public static class MountExtensions
{
	public static EmbedBuilder ToEmbed(this MountAPI.Mount self)
	{
		EmbedBuilder builder = new EmbedBuilder();
		builder.Title = self.Name;
		builder.ThumbnailUrl = self.Icon;

		StringBuilder desc = new StringBuilder();

		if (!string.IsNullOrEmpty(self.Enhanced_Description))
		{
			desc.AppendLine(self.GetDescription());
			desc.AppendLine();
		}

		builder.AddField("Movement", self.Movement, true);
		builder.AddField("Flying", self.Flying ? "Yes" : "No", true);
		builder.AddField("Owned", self.Owned, true);
		builder.AddField("Patch", self.Patch, true);

		if (self.Seats > 1)
		{
			builder.AddField("Seats", self.Seats, false);
		}

		if (self.Sources.Count > 0)
		{
			StringBuilder sourceBuilder = new StringBuilder();

			foreach (MountAPI.Mount.Source source in self.Sources)
			{
				sourceBuilder.AppendFormat("**{0}:** {1}", source.Type, source.Text);
				sourceBuilder.AppendLine();
			}

			builder.AddField("Sources", sourceBuilder);
		}

		builder.Description = desc.ToString();
		builder.Footer = new EmbedFooterBuilder();
		builder.Footer.Text = string.Format("{0}\n\nID: {1} - FFXIVCollect.com", self.Tooltip, self.ID);
		builder.Color = Color.Teal;

		return builder;
	}

	public static string GetDescription(this MountAPI.Mount self)
	{
		if (self.Enhanced_Description == null)
			return string.Empty;

		string desc = self.Enhanced_Description;
		desc = Regex.Replace(self.Enhanced_Description, "<.*?>", string.Empty);

		while (desc.Contains("\n\n"))
			desc = desc.Replace("\n\n", "\n");

		return desc;
	}
}
