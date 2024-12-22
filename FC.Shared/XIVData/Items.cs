// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.Json;
	using Discord;
	using FC.Serialization;
	using FC.Utils;

	public class Items
	{
		public static readonly List<AutocompleteResult> AutocompleteItems = [];
		public static readonly Dictionary<string, XivItem> XivItemsByName = [];
		public static readonly Dictionary<int, XivItem> XivItemsById = [];

		static Items()
		{
			// Handle numbers as strings
			JsonSerializerOptions serializerOptions = new()
			{
				NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
			};
			serializerOptions.Converters.Add(new BoolConvertor());
			serializerOptions.Converters.Add(new ObjectConvertor());

			// Parse full item list
			string fullListJson = File.ReadAllText($"{PathUtils.Current}/Assets/ItemsFull.json");
			if (fullListJson != null && !string.IsNullOrWhiteSpace(fullListJson))
			{
				var jsonList = JsonSerializer.Deserialize<List<XivItem>>(fullListJson, serializerOptions);
				if (jsonList != null)
				{
					XivItemsByName = jsonList.ToDictionary(x => x.Name, x => x);
					XivItemsById = jsonList.ToDictionary(x => x.Id, x => x);
				}
			}

			// Parse condensed item list
			string itemListJson = File.ReadAllText($"{PathUtils.Current}/Assets/ItemList.json");
			if (itemListJson != null && !string.IsNullOrWhiteSpace(itemListJson))
			{
				var jsonList = JsonSerializer.Deserialize<List<AutocompleteResult>>(itemListJson, serializerOptions);
				if (jsonList != null)
				{
					AutocompleteItems.AddRange(jsonList);
				}
			}
		}
	}
}
