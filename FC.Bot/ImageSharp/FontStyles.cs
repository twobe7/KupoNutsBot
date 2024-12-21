// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.ImageSharp
{
	using SixLabors.Fonts;

	public static class FontStyles
	{
		public static readonly TextOptions CenterText = new(Fonts.AxisRegular.CreateFont(20))
		{
			////Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};

		public static readonly TextOptions CenterBottomText = new(Fonts.AxisRegular.CreateFont(20))
		{
			////Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Bottom,
		};

		public static readonly TextOptions LeftText = new(Fonts.AxisRegular.CreateFont(20))
		{
			////Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
		};

		public static readonly TextOptions RightText = new(Fonts.AxisRegular.CreateFont(20))
		{
			////Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
		};
	}
}
