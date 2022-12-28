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
	using SixLabors.ImageSharp.PixelFormats;
	using SixLabors.ImageSharp.Processing;

	public static class CharacterCard
	{
		public static async Task<string> Draw(CharacterInfo character)
		{
			if (character.Portrait == null)
				throw new Exception("Character has no portrait");

			string portraitPath = $"{PathUtils.Current}/Temp/{character.Id}.jpg";
			await FileDownloader.Download(character.Portrait, portraitPath);

			Image backgroundImg = Image.Load(PathUtils.Current + "/Assets/CharacterCardBackground.png");

			Image<Rgba32> charImg = Image.Load<Rgba32>(portraitPath);
			charImg.Mutate(x => x.Resize(375, 512));

			Image<Rgba32> overlayImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/CharacterCardOverlay.png");

			Image<Rgba32> finalImg = new Image<Rgba32>(1024, 512);
			finalImg.Mutate(x => x.DrawImage(backgroundImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(charImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(overlayImg, 1.0f));

			// Name
			finalImg.Mutate(x => x.DrawTextAnySize(FontStyles.CenterText, character.Name, Fonts.OptimuSemiBold, Color.White, new Rectangle(680, 70, 660, 55)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.Title, Fonts.AxisRegular.CreateFont(22), Color.White, new PointF(680, 35)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, $"{character.Race} ({character.Tribe})", Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(680, 110)));

			// Grand Company
			if (character.GrandCompany != null && character.GrandCompany.Company != null)
			{
				Image<Rgba32> gcImg = Image.Load<Rgba32>($"{PathUtils.Current}/Assets/GrandCompanies/{character.GrandCompany.Company.ID}.png");
				finalImg.Mutate(x => x.DrawImage(gcImg, 1.0f));
				gcImg.Dispose();

				if (character.GrandCompany.Rank != null)
				{
					Image<Rgba32> rankImg = Image.Load<Rgba32>($"{PathUtils.Current}/Assets/GrandCompanies/Ranks/{character.GrandCompany.Rank.Icon}");
					finalImg.Mutate(x => x.DrawImage(rankImg, new Point(370, 152), 1.0f));
					rankImg.Dispose();
					finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.GrandCompany.Rank.Name, Fonts.AxisRegular.CreateFont(16), Color.White, new Point(420, 163)));
				}
			}

			// Server
			finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, $"{character.Server} - {character.DataCenter}", Fonts.AxisRegular.CreateFont(18), Color.White, new Point(412, 196)));

			// Birthday (1st Sun of the 1st Astral Moon)
			if (character.NameDay != null)
			{
				Image<Rgba32> moonImg;
				if (character.NameDay.Contains("Astral"))
				{
					moonImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/Moons/Astral.png");
				}
				else
				{
					moonImg = Image.Load<Rgba32>(PathUtils.Current + "/Assets/Moons/Umbral.png");
				}

				finalImg.Mutate(x => x.DrawImage(moonImg, new Point(907, 122), 1.0f));
				moonImg.Dispose();
			}

			Image<Rgba32> dietyImage = Image.Load<Rgba32>(PathUtils.Current + character.GuardianDeity?.Icon.Replace("/i/061000/", "/Assets/Twelve/"));
			finalImg.Mutate(x => x.DrawImage(dietyImage, new Point(907, 122), 1.0f));
			dietyImage.Dispose();

			finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.NameDay, Fonts.AxisRegular.CreateFont(16), Color.White, new Point(700, 196)));
			finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.GuardianDeity?.Name, Fonts.AxisRegular.CreateFont(20), Color.White, new Point(700, 168)));

			// Free Company
			if (character.FreeCompany != null)
			{
				Image<Rgba32> crestFinal = new Image<Rgba32>(128, 128);
				foreach (string crestPart in character.FreeCompany.Crest)
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

				crestFinal.Mutate(x => x.Resize(90, 90));
				finalImg.Mutate(x => x.DrawImage(crestFinal, new Point(482, 222), 1.0f));
				crestFinal.Dispose();

				////finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.FreeCompany.Tag, Fonts.AxisRegular.CreateFont(24), Color.White, new Point(730, 235)));
				finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, character.FreeCompany.Name + " " + character.FreeCompany.Tag, Fonts.AxisRegular.CreateFont(22), Color.White, new Point(540, 233)));
			}

			// Jobs
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Monk), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(513, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Dragoon), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(572, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Ninja), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(631, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Samurai), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(690, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Reaper), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(749, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Blackmage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(808, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Summoner), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(865, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Redmage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(925, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Bluemage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(985, 335)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Bard), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(395, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Machinist), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(454, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Dancer), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(513, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Paladin), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(572, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Warrior), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(631, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Darkknight), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(690, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Gunbreaker), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(749, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Whitemage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(808, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Scholar), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(865, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Astrologian), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(925, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Sage), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(985, 411)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Botanist), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(395, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Fisher), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(454, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Miner), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(513, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Alchemist), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(572, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Armorer), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(631, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Blacksmith), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(690, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Carpenter), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(749, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Culinarian), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(808, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Goldsmith), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(865, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Leatherworker), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(925, 485)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.GetJobLevel(Jobs.Weaver), Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(985, 485)));

			// Progress / Achievements
			if (character.HasMounts)
			{
				string mountsStr = character.Mounts.Count + " / " + character.Mounts.Total;
				float percentage = (float)character.Mounts.Count / (float)character.Mounts.Total;

				Image<Rgba32> barImg = FetchBarImage();
				float width = percentage == 0 ? 1 : percentage * barImg.Width;
				barImg.Mutate(x => x.Resize(new Size((int)width, barImg.Height)));
				finalImg.Mutate(x => x.DrawImage(barImg, new Point(385, 238), 1.0f));
				barImg.Dispose();
				finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, mountsStr, Fonts.AxisRegular.CreateFont(12), Color.White, new Point(389, 243)));
			}

			if (character.HasMinions)
			{
				string minionsStr = character.Minions.Count + " / " + character.Minions.Total;
				float percentage = (float)character.Minions.Count / (float)character.Minions.Total;

				Image<Rgba32> barImg = FetchBarImage();
				float width = percentage == 0 ? 1 : percentage * barImg.Width;
				barImg.Mutate(x => x.Resize(new Size((int)width, barImg.Height)));
				finalImg.Mutate(x => x.DrawImage(barImg, new Point(385, 270), 1.0f));
				barImg.Dispose();
				finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, minionsStr, Fonts.AxisRegular.CreateFont(12), Color.White, new Point(389, 275)));
			}

			if (character.HasAchievements)
			{
				string achieveStr = character.Achievements.Count + " / " + character.Achievements.Total;
				float percentage = (float)character.Achievements.Count / (float)character.Achievements.Total;

				Image<Rgba32> barImg = FetchBarImage();
				float width = percentage == 0 ? 1 : percentage * barImg.Width;
				barImg.Mutate(x => x.Resize(new Size((int)width, barImg.Height)));
				finalImg.Mutate(x => x.DrawImage(barImg, new Point(385, 302), 1.0f));
				barImg.Dispose();
				finalImg.Mutate(x => x.DrawText(FontStyles.LeftText, achieveStr, Fonts.AxisRegular.CreateFont(12), Color.White, new Point(389, 307)));
			}

			// Save
			string outputPath = $"{PathUtils.Current}/Temp/{character.Id}_render.png";
			finalImg.Save(outputPath);

			charImg.Dispose();
			overlayImg.Dispose();
			finalImg.Dispose();

			return outputPath;
		}

		private static Image<Rgba32> FetchBarImage()
		{
			return Image.Load<Rgba32>($"{PathUtils.Current}/Assets/Bar.png");
		}
	}
}
