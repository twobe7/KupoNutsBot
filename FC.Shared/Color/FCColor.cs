// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Color
{
	using System;

	public static class FCColor
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

		public static Discord.Color ToDiscordColor(this Colors self)
		{
			return self switch
			{
				Colors.Default => Discord.Color.Default,
				Colors.DarkerGrey => Discord.Color.DarkerGrey,
				Colors.DarkGrey => Discord.Color.DarkGrey,
				Colors.LighterGrey => Discord.Color.LighterGrey,
				Colors.DarkRed => Discord.Color.DarkRed,
				Colors.Red => Discord.Color.Red,
				Colors.DarkOrange => Discord.Color.DarkOrange,
				Colors.Orange => Discord.Color.Orange,
				Colors.LightOrange => Discord.Color.LightOrange,
				Colors.Gold => Discord.Color.Gold,
				Colors.LightGrey => Discord.Color.LightGrey,
				Colors.Magenta => Discord.Color.Magenta,
				Colors.DarkPurple => Discord.Color.DarkPurple,
				Colors.Purple => Discord.Color.Purple,
				Colors.DarkBlue => Discord.Color.DarkBlue,
				Colors.Blue => Discord.Color.Blue,
				Colors.DarkGreen => Discord.Color.DarkGreen,
				Colors.Green => Discord.Color.Green,
				Colors.DarkTeal => Discord.Color.DarkTeal,
				Colors.Teal => Discord.Color.Teal,
				Colors.DarkMagenta => Discord.Color.DarkMagenta,
				_ => throw new Exception("Unknown discord color: " + self),
			};
		}
	}
}
