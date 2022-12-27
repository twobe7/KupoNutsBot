// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using FC.Serialization;
	using FC.Utils;
	using Newtonsoft.Json;

	public class ItemAutocomplete
	{
		public static readonly List<AutocompleteResult> Items = new List<AutocompleteResult>();

		public ItemAutocomplete()
		{
			string json = File.ReadAllText(PathUtils.Current + "/Assets/ItemList.json");
			if (json != null && !string.IsNullOrWhiteSpace(json))
			{
				var jsonList = JsonConvert.DeserializeObject<List<AutocompleteResult>>(json);
				if (jsonList != null)
				{
					Items.AddRange(jsonList);
				}
			}
		}
	}
}
