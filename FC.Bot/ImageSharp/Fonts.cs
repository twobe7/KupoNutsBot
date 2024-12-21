// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.ImageSharp
{
	using FC.Utils;
	using SixLabors.Fonts;

	public static class Fonts
	{
		public static readonly FontCollection Collection = new();

		public static readonly FontFamily AxisRegular = Collection.Add($"{PathUtils.Current}/Assets/Axis-Regular.ttf");
		public static readonly FontFamily OptimuSemiBold = Collection.Add($"{PathUtils.Current}/Assets/OptimusPrincepsSemiBold.ttf");
		public static readonly FontFamily JupiterPro = Collection.Add($"{PathUtils.Current}/Assets/JupiterProFixed.ttf");
		public static readonly FontFamily Eorzea = Collection.Add($"{PathUtils.Current}/Assets/Eorzea.ttf");
	}
}
