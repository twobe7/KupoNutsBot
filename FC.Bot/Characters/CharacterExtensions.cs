// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Characters
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using Discord;
	using FC.API;
	using FC.XIVData;
	using Google.Apis.YouTube.v3.Data;
	using XIVAPI;
	using static System.Net.WebRequestMethods;
	using XIVAPICharacter = XIVAPI.Character;

	public static class CharacterExtensions
	{
		public static ClassJob? GetClassJob(this XIVAPICharacter self, Jobs id)
		{
			if (self.ClassJobs == null)
				return null;

			foreach (ClassJob job in self.ClassJobs)
			{
				if (job.Job == null)
					return null;

				if (job.Job.ID != (uint)id)
					continue;

				return job;
			}

			return null;
		}

		////public static Embed GetGear(this XIVAPICharacter self)
		////{
		////	if (self.GearSet == null || self.GearSet.Gear == null)
		////		throw new Exception("No gear set on character.");

		////	EmbedBuilder builder = new EmbedBuilder
		////	{
		////		ImageUrl = self.Portrait,
		////		ThumbnailUrl = "https://xivapi.com/" + self.GearSet.Gear.MainHand?.Item?.Icon,
		////		Title = self.Name,
		////		Description = "Average item level: " + self.GetAverageLevel().ToString(),
		////	};

		////	builder.AddGear("Main Hand", self.GearSet.Gear.MainHand, false);
		////	builder.AddGear("Off Hand", self.GearSet.Gear.OffHand, false);
		////	builder.AddGear("Head", self.GearSet.Gear.Head, true);
		////	builder.AddGear("Body", self.GearSet.Gear.Body, true);
		////	builder.AddGear("Hands", self.GearSet.Gear.Hands, true);
		////	builder.AddGear("Waist", self.GearSet.Gear.Waist, true);
		////	builder.AddGear("Legs", self.GearSet.Gear.Legs, true);
		////	builder.AddGear("Feet", self.GearSet.Gear.Feet, true);
		////	builder.AddGear("Earrings", self.GearSet.Gear.Earrings, true);
		////	builder.AddGear("Necklace", self.GearSet.Gear.Necklace, true);
		////	builder.AddGear("Bracelets", self.GearSet.Gear.Bracelets, true);
		////	builder.AddGear("Ring", self.GearSet.Gear.Ring1, true);
		////	builder.AddGear("Ring", self.GearSet.Gear.Ring2, true);

		////	return builder.Build();
		////}

		public static Embed GetGear(this XIVAPICharacter self, NetStone.Model.Parseables.Character.Gear.CharacterGear? netGear, string jobIconPath)
		{
			if (netGear == null)
				throw new Exception("No gear set on character.");

			EmbedBuilder builder = new ()
			{
				ImageUrl = self.Portrait,
				Title = self.Name,
				Description = $"Average item level: **{GetAverageLevel(netGear)}**",
			};

			// Get icon for main hand weapon
			////if (TryGetItemIconPath(netGear.Mainhand.ItemName, out string? iconUrl))
			builder.ThumbnailUrl = jobIconPath;

			builder.AddGear("Main Hand", netGear.Mainhand, false);
			builder.AddGear("Off Hand", netGear.Offhand, false);
			builder.AddGear("Head", netGear.Head, true);
			builder.AddGear("Body", netGear.Body, true);
			builder.AddGear("Hands", netGear.Hands, true);
			builder.AddGear("Waist", netGear.Waist, true);
			builder.AddGear("Legs", netGear.Legs, true);
			builder.AddGear("Feet", netGear.Feet, true);
			builder.AddGear("Earrings", netGear.Earrings, true);
			builder.AddGear("Necklace", netGear.Necklace, true);
			builder.AddGear("Bracelets", netGear.Bracelets, true);
			builder.AddGear("Ring", netGear.Ring1, true);
			builder.AddGear("Ring", netGear.Ring2, true);

			return builder.Build();
		}

		////public static int GetAverageLevel(this XIVAPICharacter self)
		////{
		////	if (self.GearSet == null || self.GearSet.Gear == null)
		////		throw new Exception("No gear set on character.");

		////	int total = 0;
		////	int count = 0;
		////	count += AddItemlevel(self.GearSet.Gear.Body?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Bracelets?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Earrings?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Feet?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Hands?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Head?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Legs?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.MainHand?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Necklace?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.OffHand?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Ring1?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Ring2?.Item, ref total) ? 1 : 0;
		////	count += AddItemlevel(self.GearSet.Gear.Waist?.Item, ref total) ? 1 : 0;

		////	double average = total / count;
		////	return (int)Math.Round(average);
		////}

		public static int GetAverageLevel(NetStone.Model.Parseables.Character.Gear.CharacterGear? netGear)
		{
			if (netGear == null)
				throw new Exception("No gear set on character.");

			int total = 0;
			int count = 0;

			count += AddItemlevel(netGear.Body?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Bracelets?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Earrings?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Feet?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Hands?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Head?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Legs?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Mainhand?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Necklace?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Offhand?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Ring1?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Ring2?.ItemName, ref total) ? 1 : 0;
			count += AddItemlevel(netGear.Waist?.ItemName, ref total) ? 1 : 0;

			double average = total / count;
			return (int)Math.Round(average);
		}

		////public static string GetString(this GearSet.GearValue self)
		////{
		////	if (self.Item == null)
		////		return string.Empty;

		////	StringBuilder builder = new ();
		////	builder.Append('[');
		////	builder.Append(self.Item.Name);
		////	builder.Append("](");
		////	builder.Append("https://garlandtools.org/db/#item/");
		////	builder.Append(self.Item.Id);

		////	if (self.Mirage != null)
		////	{
		////		builder.Append(" \"");
		////		builder.Append(self.Mirage.Name);
		////		builder.Append('"');
		////	}

		////	builder.AppendLine(")");

		////	builder.Append("iLv: ");
		////	builder.Append(self.Item.LevelItem);
		////	builder.Append(' ');

		////	foreach (Data materia in self.Materia)
		////	{
		////		builder.Append("[⬤](");
		////		builder.Append("https://garlandtools.org/db/#item/");
		////		builder.Append(materia.ID);
		////		builder.Append(" \"");
		////		builder.Append(materia.Name);
		////		builder.Append("\") ");
		////	}

		////	return builder.ToString();
		////}

		public static string ToEntryString(this NetStone.Model.Parseables.Character.Gear.GearEntry self)
		{
			if (!self.Exists)
				return string.Empty;

			StringBuilder builder = new ();

			// Get Item name with database link
			builder.Append($"[{self.ItemName}]({self.ItemDatabaseLink?.AbsoluteUri}");

			// Tooltip: Glamour if exists
			if (!string.IsNullOrWhiteSpace(self.GlamourName))
				builder.Append($" \"{self.GlamourName}\"");

			builder.AppendLine(")");

			// Get Item Level
			if (TryGetItemLevel(self.ItemName, out int? itemLevel))
				builder.AppendLine($"iLv: {itemLevel}");

			// Build materia
			Dictionary<string, int> materiaList = new ();
			foreach (string materia in self.Materia)
			{
				if (string.IsNullOrWhiteSpace(materia))
					continue;

				// Get the Item Id for Materia
				var materiaId = materiaList.TryGetValue(materia, out int existingMateria)
					? existingMateria
					: TryGetItemId(materia, out int? itemId) ? itemId : null;

				// Append to builder
				if (materiaId.HasValue)
				{
					materiaList.TryAdd(materia, materiaId.Value);

					builder.Append($"[⬤](https://garlandtools.org/db/#item/{materiaId}");
					builder.Append($" \"{materia}\")");
				}
			}

			return builder.ToString();
		}

		////public static Dictionary<AttributeCategories, List<AttributeValue>> GetAttributes(this GearSet self)
		////{
		////	Dictionary<AttributeCategories, List<AttributeValue>> result = new Dictionary<AttributeCategories, List<AttributeValue>>();
		////	foreach (AttributeValue val in self.Attributes)
		////	{
		////		AttributeCategories category = val.GetCategory();
		////		if (!result.ContainsKey(category))
		////			result.Add(category, new List<AttributeValue>());

		////		result[category].Add(val);
		////	}

		////	return result;
		////}

		////public static int GetElementalLevel(this XIVAPICharacter self)
		////{
		////	return self.ClassJobsElemental?.Level ?? 0;
		////}

		////public static int GetResistanceRank(this XIVAPICharacter self)
		////{
		////	return self.ClassJobsBozjan?.Level ?? 0;
		////}

		////public static int GetResistanceMettle(this XIVAPICharacter self)
		////{
		////	string? strMettle = self.ClassJobsBozjan?.Mettle?.ToString();
		////	return int.TryParse(strMettle, out int mettle) ? mettle : 0;
		////}

		////private static bool AddItemlevel(Item? item, ref int total)
		////{
		////	if (item == null)
		////		return false;

		////	total += item.LevelItem;
		////	return true;
		////}

		/// <summary>
		/// Gets the item level for given item by name.
		/// </summary>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="total">Running total of ilvl.</param>
		/// <returns>True when item was found, otherwise False.</returns>
		private static bool AddItemlevel(string? itemName, ref int total)
		{
			if (itemName == null)
			{
				return false;
			}
			else if (Items.XivItemsByName.TryGetValue(itemName, out XivItem? xivItem))
			{
				total += xivItem.ItemLevel;
				return true;
			}
			else if (TryGetXivApiItem(itemName, out Item? xivApiItem))
			{
				total += xivApiItem.LevelItem;
				return true;
			}

			return false;
		}

		////private static void AddGear(this EmbedBuilder builder, string name, GearSet.GearValue? gear, bool inline)
		////{
		////	if (gear == null)
		////		return;

		////	builder.AddField(name, gear.GetString(), inline);
		////}

		/// <summary>
		/// Add gear information to Embed.
		/// </summary>
		/// <param name="builder">Existing Embed.</param>
		/// <param name="name">Name of item.</param>
		/// <param name="gear">GearEntry to populate information from.</param>
		/// <param name="inline">Whether field should be inline on Embed.</param>
		private static void AddGear(this EmbedBuilder builder, string name, NetStone.Model.Parseables.Character.Gear.GearEntry? gear, bool inline)
		{
			if (gear == null)
				return;

			builder.AddField(name, gear.ToEntryString(), inline);
		}

		////private static Item? GetItem(NetStone.Model.Parseables.Character.Gear.GearEntry? gear)
		////{
		////	return gear == null
		////		? null
		////		: TryGetXivApiItem(gear.ItemName, out Item? item)
		////			? item : null;
		////}

		/// <summary>
		/// Attempts to fetch item from XIV API.
		/// </summary>
		/// <param name="itemName">Name of item to search.</param>
		/// <param name="item">Returned Item if successful.</param>
		/// <returns>True when item was found, otherwise False.</returns>
		private static bool TryGetXivApiItem(string itemName, [MaybeNullWhen(false)] out Item item)
		{
			item = null;

			List<Result> results = SearchAPI.Search(itemName, "Item").Result;
			if (results.Any())
			{
				var id = results.First().ID;
				if (id != null)
				{
					item = ItemAPI.Get((ulong)id).Result;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Attempts to return Item Icon path from internal item list or XIV API.
		/// </summary>
		/// <param name="itemName">Name of item to search.</param>
		/// <param name="iconPath">Returned icon path.</param>
		/// <returns>True when item was found, otherwise False.</returns>
		private static bool TryGetItemIconPath(string itemName, [MaybeNullWhen(false)] out string? iconPath)
		{
			iconPath = null;

			if (itemName == null)
			{
				return false;
			}
			else if (Items.XivItemsByName.TryGetValue(itemName, out XivItem? xivItem))
			{
				iconPath = xivItem.IconFullPath;
				return true;
			}
			else if (TryGetXivApiItem(itemName, out Item? xivApiItem))
			{
				iconPath = $"https://xivapi.com/{xivApiItem.Icon}";
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to return Item Level from internal item list or XIV API.
		/// </summary>
		/// <param name="itemName">Name of item to search.</param>
		/// <param name="itemLevel">Returns item level.</param>
		/// <returns>True when item was found, otherwise False.</returns>
		private static bool TryGetItemLevel(string itemName, [MaybeNullWhen(false)] out int? itemLevel)
		{
			itemLevel = null;

			if (itemName == null)
			{
				return false;
			}
			else if (Items.XivItemsByName.TryGetValue(itemName, out XivItem? xivItem))
			{
				itemLevel = xivItem.ItemLevel;
				return true;
			}
			else if (TryGetXivApiItem(itemName, out Item? xivApiItem))
			{
				itemLevel = xivApiItem.LevelItem;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to return Item Id from internal item list or XIV API.
		/// </summary>
		/// <param name="itemName">Name of item to search.</param>
		/// <param name="itemId">Returns item Id.</param>
		/// <returns>True when item was found, otherwise False.</returns>
		private static bool TryGetItemId(string itemName, [MaybeNullWhen(false)] out int? itemId)
		{
			itemId = null;

			if (itemName == null)
			{
				return false;
			}
			else if (Items.XivItemsByName.TryGetValue(itemName, out XivItem? xivItem))
			{
				itemId = xivItem.Id;
				return true;
			}
			else if (TryGetXivApiItem(itemName, out Item? xivApiItem))
			{
				itemId = xivApiItem.Id;
				return true;
			}

			return false;
		}
	}
}
