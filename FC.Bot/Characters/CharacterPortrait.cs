// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Characters
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using FC.Bot.ImageSharp;
	using FC.Bot.Utils;
	using FC.Utils;
	using SixLabors.ImageSharp;
	using SixLabors.ImageSharp.Drawing.Processing;
	using SixLabors.ImageSharp.PixelFormats;
	using SixLabors.ImageSharp.Processing;

	public static class CharacterPortrait
	{
		public static async Task<string> Draw(CharacterInfo character)
		{
			if (character.Portrait == null)
				throw new Exception("Character has no portrait");

			string portraitPath = $"{PathUtils.Current}/Temp/{character.Id}.jpg";
			await FileDownloader.Download(character.Portrait, portraitPath);

			Image<Rgba32> charImg = Image.Load<Rgba32>(portraitPath);

			Image<Rgba32> backgroundImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/CharacterPortraitBackground.png");
			backgroundImg.Mutate(x => x.Resize(charImg.Width, charImg.Height));

			Image<Rgba32> finalImg = new Image<Rgba32>(charImg.Width, charImg.Height);
			finalImg.Mutate(x => x.DrawImage(backgroundImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(charImg, 1.0f));

			PointF boxA = new PointF(5, charImg.Height - 120);
			PointF boxB = new PointF(finalImg.Width - 5, charImg.Height - 120);
			PointF boxC = new PointF(finalImg.Width - 5, charImg.Height - 5);
			PointF boxD = new PointF(5, charImg.Height - 5);

			finalImg.Mutate(x => x.FillPolygon(Brushes.Solid(Color.Black.WithAlpha(0.4F)), boxA, boxB, boxC, boxD));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, $"{character.Server} - {character.DataCenter}", Fonts.AxisRegular.CreateFont(26), Color.White, new Point(finalImg.Width / 2, charImg.Height - 95)));
			finalImg.Mutate(x => x.DrawTextAnySize(FontStyles.CenterText, character.Name, Fonts.OptimuSemiBold, Color.White, new Rectangle(finalImg.Width / 2, finalImg.Height - 50, 600, 70)));

			// Save
			string outputPath = $"{PathUtils.Current}/Temp/{character.Id}_render.png";
			finalImg.Save(outputPath);

			return outputPath;
		}
	}
}
