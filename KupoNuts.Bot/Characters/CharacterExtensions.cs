// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Discord;
	using XIVAPI;
	using XIVAPICharacter = XIVAPI.Character;

	public static class CharacterExtensions
	{
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

		public static string GetJobLevel(this XIVAPICharacter self, Jobs job)
		{
			ClassJob? classJob = self.GetClassJob(job);

			if (classJob == null)
				return string.Empty;

			return classJob.Level.ToString();
		}

		public static Embed GetGear(this XIVAPICharacter self)
		{
			if (self.GearSet == null || self.GearSet.Gear == null)
				throw new Exception("No gear set on character.");

			EmbedBuilder builder = new EmbedBuilder();
			builder.ImageUrl = self.Portrait;
			builder.ThumbnailUrl = "https://xivapi.com/" + self.GearSet.Gear.MainHand?.Item?.Icon;
			builder.Title = self.Name;
			builder.Description = "Average item level: " + self.GetAverageLevel().ToString();

			builder.AddField("Main Hand", self.GearSet.Gear.MainHand?.GetString(), false);

			if (self.GearSet.Gear.OffHand != null)
				builder.AddField("Off Hand", self.GearSet.Gear.OffHand.GetString(), false);

			builder.AddField("Head", self.GearSet.Gear.Head?.GetString(), true);
			builder.AddField("Body", self.GearSet.Gear.Body?.GetString(), true);
			builder.AddField("Hands", self.GearSet.Gear.Hands?.GetString(), true);
			builder.AddField("Waist", self.GearSet.Gear.Waist?.GetString(), true);
			builder.AddField("Legs", self.GearSet.Gear.Legs?.GetString(), true);
			builder.AddField("Feet", self.GearSet.Gear.Feet?.GetString(), true);

			builder.AddField("Earrings", self.GearSet.Gear.Earrings?.GetString(), true);
			builder.AddField("Necklace", self.GearSet.Gear.Necklace?.GetString(), true);
			builder.AddField("Bracelets", self.GearSet.Gear.Bracelets?.GetString(), true);
			builder.AddField("Ring", self.GearSet.Gear.Ring1?.GetString(), true);
			builder.AddField("Ring", self.GearSet.Gear.Ring2?.GetString(), true);

			return builder.Build();
		}

		public static int GetAverageLevel(this XIVAPICharacter self)
		{
			if (self.GearSet == null || self.GearSet.Gear == null)
				throw new Exception("No gear set on character.");

			int total = 0;
			int count = 0;
			count += AddItemlevel(self.GearSet.Gear.Body?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Bracelets?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Earrings?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Feet?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Hands?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Head?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Legs?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.MainHand?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Necklace?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.OffHand?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Ring1?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Ring2?.Item, ref total) ? 1 : 0;
			count += AddItemlevel(self.GearSet.Gear.Waist?.Item, ref total) ? 1 : 0;

			total /= count;
			return total;
		}

		public static Embed GetAttributtes(this XIVAPICharacter self)
		{
			if (self.GearSet == null || self.GearSet.Gear == null)
				throw new Exception("No gear set on character.");

			EmbedBuilder builder = new EmbedBuilder();

			Dictionary<AttributeCategories, List<AttributeValue>> attributeLookup = self.GearSet.GetAttributes();

			foreach ((AttributeCategories category, List<AttributeValue> attributes) in attributeLookup)
			{
				StringBuilder attributesBuilder = new StringBuilder();

				foreach (AttributeValue value in attributes)
					attributesBuilder.AppendLine(value.GetString());

				builder.AddField(category.ToString(), attributesBuilder.ToString(), true);
			}

			return builder.Build();
		}

		public static string GetString(this GearSet.GearValue self)
		{
			if (self.Item == null)
				return string.Empty;

			StringBuilder builder = new StringBuilder();
			builder.Append("[");
			builder.Append(self.Item.Name);
			builder.Append("](");
			builder.Append("https://garlandtools.org/db/#item/");
			builder.Append(self.Item.ID);

			if (self.Mirage != null)
			{
				builder.Append(" \"");
				builder.Append(self.Mirage.Name);
				builder.Append("\"");
			}

			builder.AppendLine(")");

			builder.Append("iLv: ");
			builder.Append(self.Item.LevelItem.ToString());
			builder.Append(" ");

			foreach (Data materia in self.Materia)
			{
				builder.Append("[⬤](");
				builder.Append("https://garlandtools.org/db/#item/");
				builder.Append(materia.ID);
				builder.Append(" \"");
				builder.Append(materia.Name);
				builder.Append("\") ");
			}

			return builder.ToString();
		}

		public static Dictionary<AttributeCategories, List<AttributeValue>> GetAttributes(this GearSet self)
		{
			Dictionary<AttributeCategories, List<AttributeValue>> result = new Dictionary<AttributeCategories, List<AttributeValue>>();
			foreach (AttributeValue val in self.Attributes)
			{
				AttributeCategories category = val.GetCategory();
				if (!result.ContainsKey(category))
					result.Add(category, new List<AttributeValue>());

				result[category].Add(val);
			}

			return result;
		}

		private static bool AddItemlevel(Item? item, ref int total)
		{
			if (item == null)
				return false;

			total += item.LevelItem;
			return true;
		}
	}
}
