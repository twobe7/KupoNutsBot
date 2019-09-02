// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Text;
	using Discord;
	using XIVAPI;

	public static class CharacterAPIExtensions
	{
		public static Embed BuildEmbed(this CharacterAPI.Character self)
		{
			EmbedBuilder builder = new EmbedBuilder();

			StringBuilder titleBuilder = new StringBuilder();

			if (self.Title != null && self.TitleTop)
			{
				titleBuilder.Append(self.Title.Name);
				titleBuilder.Append(" - ");
			}

			titleBuilder.Append(self.Name);

			if (self.Title != null && !self.TitleTop)
			{
				titleBuilder.Append(" - ");
				titleBuilder.Append(self.Title.Name);
			}

			builder.Title = titleBuilder.ToString();
			builder.ImageUrl = self.Portrait;

			StringBuilder descriptionBuilder = new StringBuilder();

			if (self.Race != null && self.Tribe != null)
			{
				descriptionBuilder.Append(self.Tribe.Name);
				descriptionBuilder.Append(", ");
				descriptionBuilder.AppendLine(self.Race.Name);
			}

			builder.Description = descriptionBuilder.ToString();

			builder.AddField("Bio", self.Bio, true);

			if (self.ClassJobs != null)
			{
				StringBuilder classes = new StringBuilder();

				int column = 0;
				foreach (CharacterAPI.ClassJob job in self.ClassJobs)
				{
					if (job.Job == null)
						continue;

					classes.Append(job.Job.Abbreviation);
					classes.Append(": **");
					classes.Append(job.Level.ToString());
					classes.Append("**");

					if (column >= 6)
					{
						classes.AppendLine();
						column = 0;
					}
					else
					{
						classes.Append("   ");
					}

					column++;
				}

				builder.AddField("Classes", classes.ToString(), true);
			}

			return builder.Build();
		}
	}
}
