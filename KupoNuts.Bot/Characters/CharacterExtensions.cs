// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Text;
	using Discord;

	using CollectCharacter = FFXIVCollect.CharacterAPI.Character;
	using XIVAPICharacter = XIVAPI.Character;

	public static class CharacterExtensions
	{
		public static Embed BuildEmbed(this XIVAPICharacter self, CollectCharacter? collect)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = Color.Blue;

			StringBuilder titleBuilder = new StringBuilder();

			if (self.Title != null && self.TitleTop)
			{
				titleBuilder.Append(self.Title.Name);
				titleBuilder.Append(" - ");
			}

			titleBuilder.Append(self.Name);

			if (self.Title != null && !self.TitleTop && !string.IsNullOrEmpty(self.Title.Name))
			{
				titleBuilder.Append(" - ");
				titleBuilder.Append(self.Title.Name);
			}

			builder.Title = titleBuilder.ToString();
			builder.ImageUrl = self.Portrait;

			builder.Footer = new EmbedFooterBuilder();
			StringBuilder footerText = new StringBuilder();
			footerText.Append("ID: ");
			footerText.Append(self.ID.ToString());
			footerText.Append(" - XIVAPI.com - FFXIVCollect.com");
			builder.Footer.Text = footerText.ToString();

			StringBuilder descriptionBuilder = new StringBuilder();

			if (self.Race != null && self.Tribe != null)
			{
				descriptionBuilder.Append(self.Tribe.Name);
				descriptionBuilder.Append(", ");
				descriptionBuilder.AppendLine(self.Race.Name);
			}

			builder.Description = descriptionBuilder.ToString();

			if (self.HasBio())
				builder.AddField("Bio", self.Bio, true);

			if (self.ClassJobs != null)
			{
				StringBuilder jobsBuilder = new StringBuilder();

				jobsBuilder.Append(self.GetJobString(Jobs.Paladin));
				jobsBuilder.Append(self.GetJobString(Jobs.Warrior));
				jobsBuilder.Append(self.GetJobString(Jobs.Darkknight));
				jobsBuilder.Append(self.GetJobString(Jobs.Gunbreaker));
				jobsBuilder.AppendLine();
				jobsBuilder.Append(self.GetJobString(Jobs.Whitemage));
				jobsBuilder.Append(self.GetJobString(Jobs.Scholar));
				jobsBuilder.Append(self.GetJobString(Jobs.Astrologian));
				jobsBuilder.AppendLine();
				jobsBuilder.Append(self.GetJobString(Jobs.Monk));
				jobsBuilder.Append(self.GetJobString(Jobs.Dragoon));
				jobsBuilder.Append(self.GetJobString(Jobs.Ninja));
				jobsBuilder.Append(self.GetJobString(Jobs.Samurai));
				jobsBuilder.AppendLine();
				jobsBuilder.Append(self.GetJobString(Jobs.Bard));
				jobsBuilder.Append(self.GetJobString(Jobs.Machinist));
				jobsBuilder.Append(self.GetJobString(Jobs.Dancer));
				jobsBuilder.AppendLine();
				jobsBuilder.Append(self.GetJobString(Jobs.Blackmage));
				jobsBuilder.Append(self.GetJobString(Jobs.Summoner));
				jobsBuilder.Append(self.GetJobString(Jobs.Redmage));
				jobsBuilder.Append(self.GetJobString(Jobs.Bluemage));

				builder.AddField("Jobs", jobsBuilder.ToString());

				StringBuilder craftersBuilder = new StringBuilder();
				craftersBuilder.Append(self.GetJobString(Jobs.Carpenter));
				craftersBuilder.Append(self.GetJobString(Jobs.Blacksmith));
				craftersBuilder.Append(self.GetJobString(Jobs.Armorer));
				craftersBuilder.Append(self.GetJobString(Jobs.Goldsmith));
				craftersBuilder.AppendLine();
				craftersBuilder.Append(self.GetJobString(Jobs.Leatherworker));
				craftersBuilder.Append(self.GetJobString(Jobs.Weaver));
				craftersBuilder.Append(self.GetJobString(Jobs.Alchemist));
				craftersBuilder.Append(self.GetJobString(Jobs.Culinarian));
				craftersBuilder.AppendLine();
				craftersBuilder.Append(self.GetJobString(Jobs.Botanist));
				craftersBuilder.Append(self.GetJobString(Jobs.Miner));
				craftersBuilder.Append(self.GetJobString(Jobs.Fisher));

				builder.AddField("Gatherers / Crafters", craftersBuilder.ToString());
			}

			if (collect != null)
				builder.AddField("Progress", collect.GetContent());

			StringBuilder linkBuilder = new StringBuilder();

			// lodestone
			linkBuilder.Append("[Lodestone](https://eu.finalfantasyxiv.com/lodestone/character/");
			linkBuilder.Append(self.ID);
			linkBuilder.AppendLine(")");

			// ffxivcollect
			if (collect == null)
			{
				linkBuilder.Append("Track achievements and more with ");
				linkBuilder.Append("[FFXIV Collect](https://ffxivcollect.com)");
			}
			else
			{
				linkBuilder.Append("[FFXIV Collect](https://ffxivcollect.com/characters/");
				linkBuilder.Append(self.ID);
				linkBuilder.AppendLine(")");
			}

			builder.AddField("Links", linkBuilder.ToString());

			return builder.Build();
		}

		public static string GetContent(this CollectCharacter collect)
		{
			StringBuilder builder = new StringBuilder();

			if (collect.Achievements != null && collect.Achievements.Count != 0)
			{
				builder.Append("Achievements: ");
				builder.AppendLine(collect.Achievements.GetContent());
			}

			if (collect.Mounts != null && collect.Mounts.Count != 0)
			{
				builder.Append("Mounts: ");
				builder.AppendLine(collect.Mounts.GetContent());
			}

			if (collect.Minions != null && collect.Minions.Count != 0)
			{
				builder.Append("Minions: ");
				builder.AppendLine(collect.Minions.GetContent());
			}

			if (collect.Orchestrions != null && collect.Orchestrions.Count != 0)
			{
				builder.Append("Orchestrions: ");
				builder.AppendLine(collect.Orchestrions.GetContent());
			}

			if (collect.Emotes != null && collect.Emotes.Count != 0)
			{
				builder.Append("Emotes: ");
				builder.AppendLine(collect.Emotes.GetContent());
			}

			if (collect.Bardings != null && collect.Bardings.Count != 0)
			{
				builder.Append("Bardings: ");
				builder.AppendLine(collect.Bardings.GetContent());
			}

			if (collect.Hairstyles != null && collect.Hairstyles.Count != 0)
			{
				builder.Append("Hairstyles: ");
				builder.AppendLine(collect.Hairstyles.GetContent());
			}

			if (collect.Triad != null && collect.Triad.Count != 0)
			{
				builder.Append("Triple Triad Cards: ");
				builder.AppendLine(collect.Triad.GetContent());
			}

			return builder.ToString();
		}

		public static string? GetContent(this FFXIVCollect.CharacterAPI.Data? data)
		{
			if (data == null)
				return null;

			if (data.Count == 0)
				return null;

			StringBuilder builder = new StringBuilder();

			builder.Append(data.Count);
			builder.Append(" / ");
			builder.Append(data.Total);

			builder.Append(" - ");
			builder.Append(Math.Round(((double)data.Count / (double)data.Total) * 100.0));
			builder.Append("%");

			return builder.ToString();
		}

		public static bool HasBio(this XIVAPICharacter self)
		{
			if (string.IsNullOrEmpty(self.Bio))
				return false;

			if (self.Bio == "-")
				return false;

			if (string.IsNullOrWhiteSpace(self.Bio))
				return false;

			return true;
		}

		public static string GetJobString(this XIVAPICharacter self, Jobs id)
		{
			XIVAPI.ClassJob? classJob = self.GetClassJob(id);

			if (classJob == null)
				return string.Empty;

			return classJob.GetDisplayString() + Utils.Characters.Tab;
		}

		public static XIVAPI.ClassJob? GetClassJob(this XIVAPICharacter self, Jobs id)
		{
			if (self.ClassJobs == null)
				return null;

			foreach (XIVAPI.ClassJob job in self.ClassJobs)
			{
				if (job.Job == null)
					return null;

				if (job.Job.ID != (uint)id)
					continue;

				return job;
			}

			return null;
		}

		public static string? GetDisplayString(this XIVAPI.ClassJob self)
		{
			StringBuilder jobsBuilder = new StringBuilder();
			if (self.Job == null)
				return null;

			Jobs job = (Jobs)(int)self.Job.ID;

			jobsBuilder.Append(job.GetEmote());
			jobsBuilder.Append(" ");

			if (self.Level > 0)
				jobsBuilder.Append("**");

			if (self.Level >= 80)
				jobsBuilder.Append("__");

			jobsBuilder.Append(self.Level.ToString());

			if (self.Level >= 80)
				jobsBuilder.Append("__");

			if (self.Level <= 0)
			{
				jobsBuilder.Append(Utils.Characters.Space);
			}
			else
			{
				jobsBuilder.Append("**");
			}

			return jobsBuilder.ToString();
		}
	}
}
