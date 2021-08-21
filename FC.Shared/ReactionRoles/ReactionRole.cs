// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.ReactionRoles
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Discord;
	using FC.Color;

	[Serializable]
	public class ReactionRole : ReactionRoleHeader
	{
		public string Name { get; set; } = "New Reaction Role";
		public string? Description { get; set; }
		public bool AppendReactionHintToMessage { get; set; }
		public FCColor.Colors Color { get; set; }

		public List<ReactionRoleItem> Reactions { get; set; } = new List<ReactionRoleItem>();

		public override string ToString()
		{
			return "Reaction Role: " + this.Name;
		}

		public Embed ToEmbed()
		{
			EmbedBuilder builder = new EmbedBuilder
			{
				Title = this.Name,
				Color = this.Color.ToDiscordColor(),
				Description = this.GetMessageDescription(),
			};

			return builder.Build();
		}

		public IEmote[] ReactionsArray()
		{
			if (this.Reactions.Count == 0)
				return new IEmote[0];

			IEmote[] array = new IEmote[this.Reactions.Count];
			for (int i = 0; i < this.Reactions.Count; i++)
				array[i] = this.Reactions[i].ReactionEmote;

			return array;
		}

		public string GetMessageDescription()
		{
			string desc = this.Description ?? string.Empty;

			// If we do not append reaction hint, return description only
			if (!this.AppendReactionHintToMessage)
				return desc;

			return desc + Environment.NewLine + Environment.NewLine + this.ReactionRoleMessage();
		}

		public string ReactionRoleMessage()
		{
			StringBuilder msg = new StringBuilder();

			if (this.Reactions.Count == 0)
				return string.Empty;

			for (int i = 0; i < this.Reactions.Count; i++)
				msg.AppendLine($"{this.Reactions[i].ReactionEmote.Name} <@&{this.Reactions[i].Role}>");

			return msg.ToString();
		}
	}
}
