// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Events
{
	using System;
	using System.Collections.Generic;
	using FC.Utils;
	using NodaTime;
	using NodaTime.Text;

	[Serializable]
	public class ReactionRole : ReactionRoleHeader
	{
		public enum Colors
		{
			Default,
			DarkerGrey,
			DarkGrey,
			LighterGrey,
			DarkRed,
			Red,
			DarkOrange,
			Orange,
			LightOrange,
			Gold,
			LightGrey,
			Magenta,
			DarkPurple,
			Purple,
			DarkBlue,
			Blue,
			DarkGreen,
			Green,
			DarkTeal,
			Teal,
			DarkMagenta,
		}

		public ulong? RoleId { get; set; }

		public string Name { get; set; } = "New Reaction Role";
		public string? ShortDescription { get; set; }
		public string? Description { get; set; }
		public string? Message { get; set; }
		public string? Image { get; set; }
		public Colors Color { get; set; }

		public override string ToString()
		{
			return "Reaction Role: " + this.Name;
		}
	}
}
