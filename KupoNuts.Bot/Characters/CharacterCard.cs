// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Buffers;
	using System.IO;
	using System.Threading.Tasks;
	using KupoNuts.Bot.ImageSharp;
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

	public static class CharacterCard
	{
		public static async Task<string> Draw(Character character, FreeCompany? freeCompany, CollectCharacter? collectCharacter)
		{
			string portraitPath = "CustomPortraits/" + character.ID + ".png";
			if (!File.Exists(portraitPath))
			{
				portraitPath = PathUtils.Current + "/Temp/" + character.ID + ".jpg";
				await FileDownloader.Download(character.Portrait, portraitPath);
			}

			Image<Rgba32> backgroundImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/CharacterCardBackground.png");

			Image<Rgba32> charImg = Image.Load<Rgba32>(portraitPath);
			charImg.Mutate(x => x.Resize(375, 512));

			Image<Rgba32> overlayImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/CharacterCardOverlay.png");

			Image<Rgba32> finalImg = new Image<Rgba32>(1024, 512);
			finalImg.Mutate(x => x.DrawImage(backgroundImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(charImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(overlayImg, 1.0f));

			// Grand Company
			if (character.GrandCompany != null && character.GrandCompany.Company != null)
			{
				Image<Rgba32> gcImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/GrandCompanies/" + character.GrandCompany.Company.ID + ".png");
				finalImg.Mutate(x => x.DrawImage(gcImg, 1.0f));
				gcImg.Dispose();

				if (character.GrandCompany.Rank != null)
				{
					Image<Rgba32> rankImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/GrandCompanies/Ranks/" + character.GrandCompany.Rank.Icon.Replace("/i/083000/", string.Empty));
					finalImg.Mutate(x => x.DrawImage(rankImg, new Point(370, 152), 1.0f));
					rankImg.Dispose();
					finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.GrandCompany?.Rank?.Name, Fonts.AxisRegular.CreateFont(18), Color.White, new Point(412, 163)));
				}
			}

			// Server
			finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.Server + " - " + character.DC, Fonts.AxisRegular.CreateFont(18), Color.White, new Point(412, 196)));

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

				finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, "<" + freeCompany.Tag + ">", Fonts.AxisRegular.CreateFont(24), Color.White, new Point(431, 300)));
				finalImg.Mutate(x => x.DrawTextAnySize(FontStyles.LeftText, freeCompany.Name, Fonts.AxisRegular, Color.White, new Rectangle(431, 280, 158, 22)));
			}

			// Name
			finalImg.Mutate(x => x.DrawTextAnySize(FontStyles.CenterText, character.Name, Fonts.OptimuSemiBold, Color.White, new Rectangle(680, 70, 660, 55)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.Title?.Name, Fonts.AxisRegular.CreateFont(22), Color.White, new PointF(680, 35)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.Race?.Name + " (" + character.Tribe?.Name + ")", Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(680, 110)));

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

			finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.Nameday, Fonts.AxisRegular.CreateFont(16), Color.White, new Point(700, 196)));
			finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.GuardianDeity?.Name, Fonts.AxisRegular.CreateFont(20), Color.White, new Point(700, 168)));

			// Jobs
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Paladin), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(631, 330)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Warrior), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(690, 330)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Darkknight), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(749, 330)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Gunbreaker), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(808, 330)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Whitemage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(865, 330)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Scholar), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(925, 330)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Astrologian), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(985, 330)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Dragoon), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(395, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Monk), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(454, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Ninja), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(513, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Samurai), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(572, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Bard), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(631, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Machinist), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(690, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Dancer), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(749, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Blackmage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(808, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Summoner), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(865, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Redmage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(925, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Bluemage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(985, 406)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Botanist), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(395, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Fisher), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(454, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Miner), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(513, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Alchemist), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(572, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Armorer), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(631, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Blacksmith), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(690, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Carpenter), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(749, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Culinarian), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(808, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Goldsmith), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(865, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Leatherworker), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(925, 480)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Weaver), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(985, 480)));

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
					finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, mountsStr, Fonts.AxisRegular.CreateFont(16), Color.White, new Point(408, 237)));
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
					finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, minionsStr, Fonts.AxisRegular.CreateFont(16), Color.White, new Point(620, 237)));
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
					finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, achieveStr, Fonts.AxisRegular.CreateFont(16), Color.White, new Point(842, 237)));
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
	}
}
