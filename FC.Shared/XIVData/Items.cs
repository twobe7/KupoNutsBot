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
		public static readonly Dictionary<string, XivItem> XivItems = new Dictionary<string, XivItem>();

		static Items()
		{
			// Parse full item list
			string fullListJson = File.ReadAllText(PathUtils.Current + "/Assets/ItemsFull.json");
			if (fullListJson != null && !string.IsNullOrWhiteSpace(fullListJson))
			{
				var jsonList = JsonConvert.DeserializeObject<List<XivItem>>(fullListJson);
				if (jsonList != null)
				{
					XivItems = jsonList.ToDictionary(x => x.Name, x => x);
				}
			}

			// Parse condensed item list
			string itemListJson = File.ReadAllText(PathUtils.Current + "/Assets/ItemList.json");
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
