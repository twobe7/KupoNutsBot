// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System.IO;
	using System.Threading.Tasks;
	using KupoNuts.Bot.Utils;
	using KupoNuts.Utils;
	using SixLabors.Fonts;
	using SixLabors.ImageSharp;
	using SixLabors.ImageSharp.PixelFormats;
	using SixLabors.ImageSharp.Processing;
	using SixLabors.Primitives;
	using XIVAPI;

	public static class PortraitDrawer
	{
		private static TextGraphicsOptions centerText = new TextGraphicsOptions()
		{
			Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};

		private static FontCollection fonts = new FontCollection();
		private static FontFamily axisRegular = fonts.Install(PathUtils.Current + "/Assets/Axis-Regular.ttf");
		private static FontFamily optimusPrincepsSemiBold = fonts.Install(PathUtils.Current + "/Assets/OptimusPrincepsSemiBold.ttf");

		public static async Task<string> Draw(Character character)
		{
			string portraitPath = PathUtils.Current + "/Temp/" + character.ID + ".jpg";
			await FileDownloader.Download(character.Portrait, portraitPath);

			Image<Rgba32> charImg = Image.Load<Rgba32>(portraitPath);
			charImg.Mutate(x => x.Resize(375, 512));

			Image<Rgba32> overlayImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/overlay.png");

			Image<Rgba32> finalImg = new Image<Rgba32>(1024, 512);
			finalImg.Mutate(x => x.DrawImage(charImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(overlayImg, 1.0f));
			finalImg.Mutate(x => x.DrawText(centerText, character.Name, optimusPrincepsSemiBold.CreateFont(45), Color.White, new PointF(768, 40)));
			finalImg.Mutate(x => x.DrawText(centerText, character.Title?.Name, axisRegular.CreateFont(25), Color.White, new PointF(768, 80)));

			string outputPath = PathUtils.Current + "/Temp/" + character.ID + "_render.jpg";
			finalImg.Save(outputPath);

			charImg.Dispose();
			overlayImg.Dispose();
			finalImg.Dispose();

			return outputPath;
		}
	}
}
