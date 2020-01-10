// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.ImageSharp
{
	using FC.Utils;
	using SixLabors.Fonts;

	public static class Fonts
	{
		public static FontCollection Collection = new FontCollection();

		public static FontFamily AxisRegular = Collection.Install(PathUtils.Current + "/Assets/Axis-Regular.ttf");
		public static FontFamily OptimuSemiBold = Collection.Install(PathUtils.Current + "/Assets/OptimusPrincepsSemiBold.ttf");
		public static FontFamily JupiterPro = Collection.Install(PathUtils.Current + "/Assets/JupiterProFixed.ttf");
		public static FontFamily Eorzea = Collection.Install(PathUtils.Current + "/Assets/Eorzea.ttf");
	}
}
