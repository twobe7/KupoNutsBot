// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.ReactionRoles
{
	using System;
	using Discord;

	[Serializable]
	public class ReactionRoleItem : ReactionRoleHeader
	{
		public string ReactionRoleId { get; set; } = string.Empty;

		public string? Reaction { get; set; }
		public string? ReactionName { get; set; }

		public ulong? Role { get; set; }
		public string RoleName { get; set; } = string.Empty;

		public string Url { get; set; } = string.Empty;
		public bool IsStandard { get; set; }

		public IEmote ReactionEmote
		{
			get
			{
				try
				{
					return Emote.Parse(this.Reaction);
				}
				catch
				{
					return new Emoji(this.Reaction);
				}
			}
		}
	}
}
