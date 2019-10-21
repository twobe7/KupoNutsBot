// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
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

		public static async Task<string> Draw(Character character, FreeCompany? freeCompany)
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
			finalImg.Mutate(x => x.DrawImage(rankImg, new Point(370, 55), 1.0f));
			rankImg.Dispose();
			finalImg.Mutate(x => x.DrawText(leftText, character.GrandCompany?.Rank?.Name, axisRegular.CreateFont(20), Color.White, new Point(420, 64)));

			// Free Company
			if (freeCompany != null)
			{
				foreach (string crestPart in freeCompany.Crest)
				{
					string name = Path.GetFileName(crestPart);
					string crestPath = PathUtils.Current + "/Crests/" + name;

					if (!File.Exists(crestPath))
						await FileDownloader.Download(crestPart, crestPath);

					Image<Rgba32> crestImg = Image.Load<Rgba32>(crestPath);
					crestImg.Mutate(x => x.Resize(75, 75));
					finalImg.Mutate(x => x.DrawImage(crestImg, new Point(936, 12), 1.0f));
					crestImg.Dispose();
				}

				finalImg.Mutate(x => x.DrawText(rightText, "<" + freeCompany.Tag + ">", axisRegular.CreateFont(20), Color.White, new Point(925, 40)));
				finalImg.Mutate(x => x.DrawText(rightText, freeCompany.Name, axisRegular.CreateFont(20), Color.White, new Point(925, 64)));
			}

			// Name / Bio
			finalImg.Mutate(x => x.DrawTextAnySize(centerText, character.Name, optimuSemiBold, Color.White, new Rectangle(182, 450, 350, 50)));
			finalImg.Mutate(x => x.DrawText(centerText, character.Title?.Name, axisRegular.CreateFont(22), Color.White, new PointF(182, 420)));
			finalImg.Mutate(x => x.DrawText(centerText, character.Race?.Name + " (" + character.Tribe?.Name + ")", axisRegular.CreateFont(20), Color.White, new PointF(182, 485)));
			finalImg.Mutate(x => x.DrawText(leftText, character.Bio, axisRegular.CreateFont(20), Color.White, new Rectangle(370, 425, 630, 74)));

			// Jobs
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Paladin), axisRegular.CreateFont(20), Color.White, new PointF(631, 225)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Warrior), axisRegular.CreateFont(20), Color.White, new PointF(690, 225)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Darkknight), axisRegular.CreateFont(20), Color.White, new PointF(749, 225)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Gunbreaker), axisRegular.CreateFont(20), Color.White, new PointF(810, 225)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Whitemage), axisRegular.CreateFont(20), Color.White, new PointF(865, 225)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Scholar), axisRegular.CreateFont(20), Color.White, new PointF(925, 225)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Astrologian), axisRegular.CreateFont(20), Color.White, new PointF(985, 225)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Dragoon), axisRegular.CreateFont(20), Color.White, new PointF(395, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Monk), axisRegular.CreateFont(20), Color.White, new PointF(454, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Ninja), axisRegular.CreateFont(20), Color.White, new PointF(513, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Samurai), axisRegular.CreateFont(20), Color.White, new PointF(572, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Bard), axisRegular.CreateFont(20), Color.White, new PointF(631, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Machinist), axisRegular.CreateFont(20), Color.White, new PointF(690, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Dancer), axisRegular.CreateFont(20), Color.White, new PointF(749, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Blackmage), axisRegular.CreateFont(20), Color.White, new PointF(810, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Summoner), axisRegular.CreateFont(20), Color.White, new PointF(865, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Redmage), axisRegular.CreateFont(20), Color.White, new PointF(925, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Bluemage), axisRegular.CreateFont(20), Color.White, new PointF(985, 310)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Botanist), axisRegular.CreateFont(20), Color.White, new PointF(395, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Fisher), axisRegular.CreateFont(20), Color.White, new PointF(454, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Miner), axisRegular.CreateFont(20), Color.White, new PointF(513, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Alchemist), axisRegular.CreateFont(20), Color.White, new PointF(572, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Armorer), axisRegular.CreateFont(20), Color.White, new PointF(631, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Blacksmith), axisRegular.CreateFont(20), Color.White, new PointF(690, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Carpenter), axisRegular.CreateFont(20), Color.White, new PointF(749, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Culinarian), axisRegular.CreateFont(20), Color.White, new PointF(810, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Goldsmith), axisRegular.CreateFont(20), Color.White, new PointF(865, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Leatherworker), axisRegular.CreateFont(20), Color.White, new PointF(925, 395)));
			finalImg.Mutate(x => x.DrawText(centerText, GetJob(character, Jobs.Weaver), axisRegular.CreateFont(20), Color.White, new PointF(985, 395)));

			// Server
			finalImg.Mutate(x => x.DrawText(leftText, character.Server + " - " + character.DC, axisRegular.CreateFont(18), new Color(new Argb32(1f, 1f, 1f, 0.5f)), new PointF(40, 10)));

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
