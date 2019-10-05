// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Pages
{
	using System;
	using Discord;

	public enum Navigation
	{
		None,

		Up,
		Down,
		Left,
		Right,
		Yes,
		No,
	}

	#pragma warning disable SA1649 // File name should match first type name
	public static class NavigationExtensions
	{
		private static IEmote cancelEmote = Emote.Parse("<:No:604942582589423618>");
		private static IEmote confirmEmote = Emote.Parse("<:Yes:604942582866247690>");
		private static IEmote upEmote = new Emoji("\u2B06");
		private static IEmote downEmote = new Emoji("\u2B07");
		private static IEmote leftEmote = new Emoji("\u2B05");
		private static IEmote rightEmote = new Emoji("\u27A1");

		public static IEmote ToEmote(this Navigation navigation)
		{
			switch (navigation)
			{
				case Navigation.Up: return upEmote;
				case Navigation.Down: return downEmote;
				case Navigation.Left: return leftEmote;
				case Navigation.Right: return rightEmote;
				case Navigation.Yes: return confirmEmote;
				case Navigation.No: return cancelEmote;
			}

			throw new Exception("Unknown navigation: " + navigation);
		}

		public static Navigation GetNavigation(IEmote emote)
		{
			if (emote.Name == upEmote.Name)
				return Navigation.Up;

			if (emote.Name == downEmote.Name)
				return Navigation.Down;

			if (emote.Name == leftEmote.Name)
				return Navigation.Left;

			if (emote.Name == rightEmote.Name)
				return Navigation.Right;

			if (emote.Name == confirmEmote.Name)
				return Navigation.Yes;

			if (emote.Name == cancelEmote.Name)
				return Navigation.No;

			return Navigation.None;
		}
	}
}
