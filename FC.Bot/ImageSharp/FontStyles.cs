// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.ImageSharp
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
