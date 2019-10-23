// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Buffers;
	using System.IO;
	using System.Threading.Tasks;
	using KupoNuts.Bot.Utils;
	using KupoNuts.Utils;
	using SixLabors.Fonts;
	using SixLabors.ImageSharp;
	using SixLabors.ImageSharp.Memory;
	using SixLabors.ImageSharp.PixelFormats;
	using SixLabors.ImageSharp.Processing;
	using SixLabors.ImageSharp.Processing.Processors;
	using SixLabors.Primitives;
	using XIVAPI;

	using CollectCharacter = FFXIVCollect.CharacterAPI.Character;

	public static class PortraitDrawer
	{
		private static TextGraphicsOptions centerText = new TextGraphicsOptions()
		{
			Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};

		private static TextGraphicsOptions leftText = new TextGraphicsOptions()
		{
			Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
		};

		private static TextGraphicsOptions rightText = new TextGraphicsOptions()
		{
			Antialias = true,
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
		};

		private static FontCollection fonts = new FontCollection();
		private static FontFamily axisRegular = fonts.Install(PathUtils.Current + "/Assets/Axis-Regular.ttf");
		private static FontFamily optimuSemiBold = fonts.Install(PathUtils.Current + "/Assets/OptimusPrincepsSemiBold.ttf");
		private static FontFamily jupiterPro = fonts.Install(PathUtils.Current + "/Assets/JupiterProFixed.ttf");

		public static async Task<string> Draw(Character character, FreeCompany? freeCompany, CollectCharacter? collectCharacter)
		{
			string portraitPath = PathUtils.Current + "/Temp/" + character.ID + ".jpg";
			await FileDownloader.Download(character.Portrait, portraitPath);

			Image<Rgba32> charImg = Image.Load<Rgba32>(portraitPath);
			charImg.Mutate(x => x.Resize(375, 512));

			Image<Rgba32> overlayImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/overlay.png");

			Image<Rgba32> finalImg = new Image<Rgba32>(1024, 512);
			finalImg.Mutate(x => x.DrawImage(charImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(overlayImg, 1.0f));

			// Grand Company
			Image<Rgba32> gcImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/GrandCompanies/" + character.GrandCompany?.Company?.ID + ".png");
			finalImg.Mutate(x => x.DrawImage(gcImg, 1.0f));
			gcImg.Dispose();

			Image<Rgba32> rankImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/GrandCompanies/Ranks/" + character.GrandCompany?.Rank?.Icon.Replace("/i/083000/", string.Empty));
			finalImg.Mutate(x => x.DrawImage(rankImg, new Point(370, 152), 1.0f));
			rankImg.Dispose();
			finalImg.Mutate(x => x.DrawText(leftText, character.GrandCompany?.Rank?.Name, axisRegular.CreateFont(18), Color.White, new Point(412, 163)));

			// Server
			finalImg.Mutate(x => x.DrawText(leftText, character.Server + " - " + character.DC, axisRegular.CreateFont(18), Color.White, new Point(412, 196)));

			// Free Company
			if (freeCompany != null)
			{
				Image<Rgba32> crestFinal = new Image<Rgba32>(128, 128);
				foreach (string crestPart in freeCompany.Crest)
				{
					string name = Path.GetFileName(crestPart);
					string crestPath = PathUtils.Current + "/Crests/" + name;

					if (!File.Exists(crestPath))
						await FileDownloader.Download(crestPart, crestPath);

					Image<Rgba32> crestImg = Image.Load<Rgba32>(crestPath);
					crestFinal.Mutate(x => x.DrawImage(crestImg, 1.0f));
					crestImg.Dispose();
				}

				for (int y = 0; y < crestFinal.Height; y++)
				{
					for (int x = 0; x < crestFinal.Width; x++)
					{
						Rgba32 pixel = crestFinal[x, y];

						if (pixel.R == 64 && pixel.G == 64 && pixel.B == 64)
							pixel.A = 0;

						crestFinal[x, y] = pixel;
					}
				}

				crestFinal.Mutate(x => x.Resize(64, 64));
				finalImg.Mutate(x => x.DrawImage(crestFinal, new Point(364, 270), 1.0f));
				crestFinal.Dispose();

				finalImg.Mutate(x => x.DrawText(leftText, "<" + freeCompany.Tag + ">", axisRegular.CreateFont(24), Color.White, new Point(431, 300)));
				finalImg.Mutate(x => x.DrawTextAnySize(leftText, freeCompany.Name, axisRegular, Color.White, new Rectangle(431, 280, 158, 32)));
			}

			// Name / Bio
			finalImg.Mutate(x => x.DrawTextAnySize(centerText, character.Name, optimuSemiBold, Color.White, new Rectangle(680, 70, 660, 55)));
			finalImg.Mutate(x => x.DrawText(centerText, character.Title?.Name, axisRegular.CreateFont(22), Color.White, new PointF(680, 35)));
			finalImg.Mutate(x => x.DrawText(centerText, character.Race?.Name + " (" + character.Tribe?.Name + ")", axisRegular.CreateFont(20), Color.White, new PointF(680, 110)));

			// Birthday (1st Sun of the 1st Astral Moon)
			Image<Rgba32> moonImg;
			if (character.Nameday.Contains("Astral"))
			{
				moonImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/Moons/Astral.png");
			}
			else
			{
				moonImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/Moons/Umbral.png");
			}

			finalImg.Mutate(x => x.DrawImage(moonImg, new Point(907, 122), 1.0f));
			moonImg.Dispose();

			Image<Rgba32> dietyImage = Image.Load<Rgba32>(PathUtils.Current + character.GuardianDeity?.Icon.Replace("/i/061000/", "/Assets/Twelve/"));
			finalImg.Mutate(x => x.DrawImage(dietyImage, new Point(907, 122), 1.0f));
			dietyImage.Dispose();

			finalImg.Mutate(x => x.DrawText(leftText, character.Nameday, axisRegular.CreateFont(16), Color.White, new Point(700, 196)));
			finalImg.Mutate(x => x.DrawText(leftText, character.GuardianDeity?.Name, jupiterPro.CreateFont(18), Color.White, new Point(700, 170)));

			// Jobs
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Paladin), axisRegular.CreateFont(20), Color.White, new PointF(631, 330)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Warrior), axisRegular.CreateFont(20), Color.White, new PointF(690, 330)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Darkknight), axisRegular.CreateFont(20), Color.White, new PointF(749, 330)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Gunbreaker), axisRegular.CreateFont(20), Color.White, new PointF(808, 330)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Whitemage), axisRegular.CreateFont(20), Color.White, new PointF(865, 330)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Scholar), axisRegular.CreateFont(20), Color.White, new PointF(925, 330)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Astrologian), axisRegular.CreateFont(20), Color.White, new PointF(985, 330)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Dragoon), axisRegular.CreateFont(20), Color.White, new PointF(395, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Monk), axisRegular.CreateFont(20), Color.White, new PointF(454, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Ninja), axisRegular.CreateFont(20), Color.White, new PointF(513, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Samurai), axisRegular.CreateFont(20), Color.White, new PointF(572, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Bard), axisRegular.CreateFont(20), Color.White, new PointF(631, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Machinist), axisRegular.CreateFont(20), Color.White, new PointF(690, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Dancer), axisRegular.CreateFont(20), Color.White, new PointF(749, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Blackmage), axisRegular.CreateFont(20), Color.White, new PointF(808, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Summoner), axisRegular.CreateFont(20), Color.White, new PointF(865, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Redmage), axisRegular.CreateFont(20), Color.White, new PointF(925, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Bluemage), axisRegular.CreateFont(20), Color.White, new PointF(985, 406)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Botanist), axisRegular.CreateFont(20), Color.White, new PointF(395, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Fisher), axisRegular.CreateFont(20), Color.White, new PointF(454, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Miner), axisRegular.CreateFont(20), Color.White, new PointF(513, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Alchemist), axisRegular.CreateFont(20), Color.White, new PointF(572, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Armorer), axisRegular.CreateFont(20), Color.White, new PointF(631, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Blacksmith), axisRegular.CreateFont(20), Color.White, new PointF(690, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Carpenter), axisRegular.CreateFont(20), Color.White, new PointF(749, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Culinarian), axisRegular.CreateFont(20), Color.White, new PointF(808, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Goldsmith), axisRegular.CreateFont(20), Color.White, new PointF(865, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Leatherworker), axisRegular.CreateFont(20), Color.White, new PointF(925, 480)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Weaver), axisRegular.CreateFont(20), Color.White, new PointF(985, 480)));

			// Progress
			if (collectCharacter != null)
			{
				if (collectCharacter.Mounts != null)
				{
					string mountsStr = collectCharacter.Mounts.Count + " / " + collectCharacter.Mounts.Total;
					float p = (float)collectCharacter.Mounts.Count / (float)collectCharacter.Mounts.Total;

					Image<Rgba32> barImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/Bar.png");
					float width = p * barImg.Width;
					barImg.Mutate(x => x.Resize(new Size((int)width, barImg.Height)));
					finalImg.Mutate(x => x.DrawImage(barImg, new Point(404, 234), 1.0f));
					barImg.Dispose();
					finalImg.Mutate(x => x.DrawText(leftText, mountsStr, axisRegular.CreateFont(16), Color.White, new Point(408, 237)));
				}

				if (collectCharacter.Minions != null)
				{
					string minionsStr = collectCharacter.Minions.Count + " / " + collectCharacter.Minions.Total;
					float p = (float)collectCharacter.Minions.Count / (float)collectCharacter.Minions.Total;

					Image<Rgba32> barImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/Bar.png");
					float width = p * barImg.Width;
					barImg.Mutate(x => x.Resize(new Size((int)width, barImg.Height)));
					finalImg.Mutate(x => x.DrawImage(barImg, new Point(616, 234), 1.0f));
					barImg.Dispose();
					finalImg.Mutate(x => x.DrawText(leftText, minionsStr, axisRegular.CreateFont(16), Color.White, new Point(620, 237)));
				}

				if (collectCharacter.Achievements != null)
				{
					string achieveStr = collectCharacter.Achievements.Count + " / " + collectCharacter.Achievements.Total;
					float p = (float)collectCharacter.Achievements.Count / (float)collectCharacter.Achievements.Total;

					Image<Rgba32> barImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/Bar.png");
					float width = p * barImg.Width;
					barImg.Mutate(x => x.Resize(new Size((int)width, barImg.Height)));
					finalImg.Mutate(x => x.DrawImage(barImg, new Point(838, 234), 1.0f));
					barImg.Dispose();
					finalImg.Mutate(x => x.DrawText(leftText, achieveStr, axisRegular.CreateFont(16), Color.White, new Point(842, 237)));
				}
			}

			// Save
			string outputPath = PathUtils.Current + "/Temp/" + character.ID + "_render.png";
			finalImg.Save(outputPath);

			charImg.Dispose();
			overlayImg.Dispose();
			finalImg.Dispose();

			return outputPath;
		}

		private static string GetJob(Character character, Jobs job)
		{
			ClassJob? classJob = character.GetClassJob(job);

			if (classJob == null)
				return string.Empty;

			return classJob.Level.ToString();
		}

		private static void DrawText(this IImageProcessingContext context, TextGraphicsOptions op, string text, Font font, Color color, Rectangle bounds)
		{
			op.WrapTextWidth = bounds.Width;

			RendererOptions rOp = new RendererOptions(font);
			rOp.WrappingWidth = bounds.Width;

			bool fits = false;
			while (!fits)
			{
				SizeF size = TextMeasurer.Measure(text, rOp);
				fits = size.Height <= bounds.Height && size.Width <= bounds.Width;

				if (!fits)
				{
					text = text.Truncate(text.Length - 5);
				}
			}

			context.DrawText(op, text, font, color, new Point(bounds.X, bounds.Y));
		}

		private static void DrawTextAnySize(this IImageProcessingContext context, TextGraphicsOptions op, string text, FontFamily font, Color color, Rectangle bounds)
		{
			int fontSize = 64;
			bool fits = false;
			Font currentFont = font.CreateFont(fontSize);
			while (!fits)
			{
				currentFont = font.CreateFont(fontSize);
				SizeF size = TextMeasurer.Measure(text, new RendererOptions(currentFont));
				fits = size.Height <= bounds.Height && size.Width <= bounds.Width;

				if (!fits)
				{
					fontSize -= 2;
				}

				if (fontSize <= 2)
				{
					return;
				}
			}

			context.DrawText(op, text, currentFont, color, new Point(bounds.X, bounds.Y));
		}
	}
}
