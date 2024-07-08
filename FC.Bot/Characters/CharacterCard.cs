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

			Image<Rgba32> finalImg = new (1024, 512);
			finalImg.Mutate(x => x.DrawImage(backgroundImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(charImg, 1.0f));
			finalImg.Mutate(x => x.DrawImage(overlayImg, 1.0f));

			// Active Job
			if (!string.IsNullOrWhiteSpace(character.ActiveClassJobIconPath))
			{
				string activeJobPath = $"{PathUtils.Current}/Temp/{character.Id}_activeJob.jpg";
				await FileDownloader.Download(character.ActiveClassJobIconPath, activeJobPath);
				Image<Rgba32> activeJobImg = Image.Load<Rgba32>(activeJobPath);
				activeJobImg.Mutate(x => x.Resize(60, 60));
				finalImg.Mutate(x => x.DrawImage(activeJobImg, new Point(20, 440), 0.8f));
				activeJobImg.Dispose();
			}

			// Name
			finalImg.Mutate(x => x.DrawTextAnySize(FontStyles.CenterText, character.Name, Fonts.OptimuSemiBold, Color.White, new Rectangle(680, 70, 660, 55)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, character.Title, Fonts.AxisRegular.CreateFont(22), Color.White, new PointF(680, 35)));
			finalImg.Mutate(x => x.DrawText(FontStyles.CenterText, $"{character.Race} ({character.Tribe})", Fonts.AxisRegular.CreateFont(20), Color.White, new PointF(680, 110)));

			// Grand Company
			if (character.GrandCompany?.Company != null)
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
				var nameDayPath = PathUtils.Current + $"/Assets/Moons/{(character.NameDay.Contains("Astral") ? "Astral" : "Umbral")}.png";
				Image<Rgba32> moonImg = Image.Load<Rgba32>(nameDayPath);

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
				Image<Rgba32> crestFinal = new (128, 128);
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

				// Render Crest
				crestFinal.Mutate(x => x.Resize(88, 88));
				finalImg.Mutate(x => x.DrawImage(crestFinal, new Point(686, 222), 1.0f));
				crestFinal.Dispose();

				// Render FC Name/Tag
				finalImg.Mutate(x => x.DrawText(
					FontStyles.LeftText,
					$"{character.FreeCompany.Name} {character.FreeCompany.Tag}",
					Fonts.AxisRegular.CreateFont(18),
					Color.White,
					new Point(738, 236)));
			}

			// TODO: Move
			var firstRow = 335;
			var secondRow = 411;
			var thirdRow = 485;

			var col_1 = 395;
			var col_2 = col_1 + 59;
			var col_3 = col_2 + 59;
			var col_4 = col_3 + 59;
			var col_5 = col_4 + 59;
			var col_6 = col_5 + 59;
			var col_7 = col_6 + 59;
			var col_8 = col_7 + 59;
			var col_9 = col_8 + 59;
			var col_10 = col_9 + 59;
			var col_11 = col_10 + 59;

			// Jobs
			RenderJob(finalImg, character, Jobs.Monk, new PointF(395, firstRow));
			RenderJob(finalImg, character, Jobs.Dragoon, new PointF(454, firstRow));
			RenderJob(finalImg, character, Jobs.Ninja, new PointF(513, firstRow));
			RenderJob(finalImg, character, Jobs.Samurai, new PointF(572, firstRow));
			RenderJob(finalImg, character, Jobs.Reaper, new PointF(631, firstRow));
			RenderJob(finalImg, character, Jobs.Viper, new PointF(690, firstRow));
			RenderJob(finalImg, character, Jobs.Blackmage, new PointF(749, firstRow));
			RenderJob(finalImg, character, Jobs.Summoner, new PointF(808, firstRow));
			RenderJob(finalImg, character, Jobs.Redmage, new PointF(865, firstRow));
			RenderJob(finalImg, character, Jobs.Pictomancer, new PointF(925, firstRow));
			RenderJob(finalImg, character, Jobs.Bluemage, new PointF(985, firstRow));
			RenderJob(finalImg, character, Jobs.Bard, new PointF(395, secondRow));
			RenderJob(finalImg, character, Jobs.Machinist, new PointF(454, secondRow));
			RenderJob(finalImg, character, Jobs.Dancer, new PointF(513, secondRow));
			RenderJob(finalImg, character, Jobs.Paladin, new PointF(572, secondRow));
			RenderJob(finalImg, character, Jobs.Warrior, new PointF(631, secondRow));
			RenderJob(finalImg, character, Jobs.Darkknight, new PointF(690, secondRow));
			RenderJob(finalImg, character, Jobs.Gunbreaker, new PointF(749, secondRow));
			RenderJob(finalImg, character, Jobs.Whitemage, new PointF(808, secondRow));
			RenderJob(finalImg, character, Jobs.Scholar, new PointF(865, secondRow));
			RenderJob(finalImg, character, Jobs.Astrologian, new PointF(925, secondRow));
			RenderJob(finalImg, character, Jobs.Sage, new PointF(985, secondRow));
			RenderJob(finalImg, character, Jobs.Botanist, new PointF(395, thirdRow));
			RenderJob(finalImg, character, Jobs.Fisher, new PointF(454, thirdRow));
			RenderJob(finalImg, character, Jobs.Miner, new PointF(513, thirdRow));
			RenderJob(finalImg, character, Jobs.Alchemist, new PointF(572, thirdRow));
			RenderJob(finalImg, character, Jobs.Armorer, new PointF(631, thirdRow));
			RenderJob(finalImg, character, Jobs.Blacksmith, new PointF(690, thirdRow));
			RenderJob(finalImg, character, Jobs.Carpenter, new PointF(749, thirdRow));
			RenderJob(finalImg, character, Jobs.Culinarian, new PointF(808, thirdRow));
			RenderJob(finalImg, character, Jobs.Goldsmith, new PointF(865, thirdRow));
			RenderJob(finalImg, character, Jobs.Leatherworker, new PointF(925, thirdRow));
			RenderJob(finalImg, character, Jobs.Weaver, new PointF(985, thirdRow));

			// Progress / Achievements
			if (character.HasMounts)
			{
				RenderProgress(
					finalImg,
					character.Mounts.Count,
					character.Mounts.Total,
					new Point(379, 235),
					new Point(384, 240));
			}

			if (character.HasMinions)
			{
				RenderProgress(
					finalImg,
					character.Minions.Count,
					character.Minions.Total,
					new Point(486, 235),
					new Point(491, 240));
			}

			if (character.HasAchievements)
			{
				RenderProgress(
					finalImg,
					character.Achievements.Count,
					character.Achievements.Total,
					new Point(593, 235),
					new Point(598, 240));
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

		private static void RenderJob(Image<Rgba32> image, CharacterInfo character, Jobs job, PointF point)
		{
			image.Mutate(x => x.DrawText(
				FontStyles.CenterText,
				character.GetJobLevel(job),
				Fonts.AxisRegular.CreateFont(20),
				Color.White,
				point));
		}

		private static void RenderProgress(Image<Rgba32> finalImg, int count, int total, Point barPoint, Point textPoint)
		{
			float percentage = (float)count / (float)total;

			Image<Rgba32> barImg = FetchBarImage();
			float width = percentage == 0 ? 1 : percentage * (barImg.Width - 2);
			barImg.Mutate(x => x.Resize(new Size((int)width, barImg.Height)));
			finalImg.Mutate(x => x.DrawImage(barImg, barPoint, 1.0f));
			barImg.Dispose();

			finalImg.Mutate(x => x.DrawText(
				FontStyles.LeftText,
				$"{count} / {total}",
				Fonts.AxisRegular.CreateFont(12),
				Color.White,
				textPoint));
		}
	}
}
