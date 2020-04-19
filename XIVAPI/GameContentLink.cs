// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Serializable]

	public class GameContentLink
	{
		public Link? FurnitureCatalogItemList { get; set; }
		public Link? HousingFurniture { get; set; }
		public Link? Recipe { get; set; }

		[Serializable]
		public class Link
		{
			public List<int> Item { get; set; } = new List<int>();
			public List<int> ItemResult { get; set; } = new List<int>();
		}
	}
}
