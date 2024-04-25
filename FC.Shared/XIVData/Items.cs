// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Discord;
	using FC.Utils;
	using Newtonsoft.Json;

	public class Items
	{
		public static readonly List<AutocompleteResult> AutocompleteItems = new List<AutocompleteResult>();
		public static readonly Dictionary<string, XivItem> XivItemsByName = new Dictionary<string, XivItem>();
		public static readonly Dictionary<int, XivItem> XivItemsById = new Dictionary<int, XivItem>();

		static Items()
		{
			// TODO: Fix this
			// Parse full item list
			string fullListJson;
#if !DEBUG
			fullListJson = File.ReadAllText(PathUtils.Current + "/Assets/ItemsFull.json");			
#else
			fullListJson = File.ReadAllText(PathUtils.Current + "..//Assets/ItemsFull.json");
#endif
			if (fullListJson != null && !string.IsNullOrWhiteSpace(fullListJson))
			{
				var jsonList = JsonConvert.DeserializeObject<List<XivItem>>(fullListJson);
				if (jsonList != null)
				{
					XivItemsByName = jsonList.ToDictionary(x => x.Name, x => x);
					XivItemsById = jsonList.ToDictionary(x => x.Id, x => x);
				}
			}

			// TODO: Fix this
			// Parse condensed item list
			string itemListJson;
#if !DEBUG
			itemListJson = File.ReadAllText(PathUtils.Current + "/Assets/ItemList.json");			
#else
			itemListJson = File.ReadAllText(PathUtils.Current + "..//Assets/ItemList.json");
#endif
			if (itemListJson != null && !string.IsNullOrWhiteSpace(itemListJson))
			{
				var jsonList = JsonConvert.DeserializeObject<List<AutocompleteResult>>(itemListJson);
				if (jsonList != null)
				{
					AutocompleteItems.AddRange(jsonList);
				}
			}
		}
	}
}