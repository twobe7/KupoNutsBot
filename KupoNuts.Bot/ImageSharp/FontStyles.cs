// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.ImageSharp
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using SixLabors.Fonts;
	using SixLabors.ImageSharp.Processing;

	public static class FontStyles
	{
		public static TextGraphicsOptions CenterText = new TextGraphicsOptions()
		{
			Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};

		public static TextGraphicsOptions LeftText = new TextGraphicsOptions()
		{
			Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
		};

		public static TextGraphicsOptions RightText = new TextGraphicsOptions()
		{
			Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
		};
	}
}
