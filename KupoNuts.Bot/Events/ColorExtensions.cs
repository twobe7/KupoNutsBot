// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
	using System;
	using KupoNuts.Events;

	public static class ColorExtensions
	{
		public static Discord.Color ToDiscordColor(this Event.Colors self)
		{
			switch (self)
			{
				case Event.Colors.Default: return Discord.Color.Default;
				case Event.Colors.DarkerGrey: return Discord.Color.DarkerGrey;
				case Event.Colors.DarkGrey: return Discord.Color.DarkGrey;
				case Event.Colors.LighterGrey: return Discord.Color.LighterGrey;
				case Event.Colors.DarkRed: return Discord.Color.DarkRed;
				case Event.Colors.Red: return Discord.Color.Red;
				case Event.Colors.DarkOrange: return Discord.Color.DarkOrange;
				case Event.Colors.Orange: return Discord.Color.Orange;
				case Event.Colors.LightOrange: return Discord.Color.LightOrange;
				case Event.Colors.Gold: return Discord.Color.Gold;
				case Event.Colors.LightGrey: return Discord.Color.LightGrey;
				case Event.Colors.Magenta: return Discord.Color.Magenta;
				case Event.Colors.DarkPurple: return Discord.Color.DarkPurple;
				case Event.Colors.Purple: return Discord.Color.Purple;
				case Event.Colors.DarkBlue: return Discord.Color.DarkBlue;
				case Event.Colors.Blue: return Discord.Color.Blue;
				case Event.Colors.DarkGreen: return Discord.Color.DarkGreen;
				case Event.Colors.Green: return Discord.Color.Green;
				case Event.Colors.DarkTeal: return Discord.Color.DarkTeal;
				case Event.Colors.Teal: return Discord.Color.Teal;
				case Event.Colors.DarkMagenta: return Discord.Color.DarkMagenta;
			}

			throw new Exception("Unknown discord color: " + self);
		}
	}
}
