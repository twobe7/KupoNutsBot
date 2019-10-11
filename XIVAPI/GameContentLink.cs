// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Serializable]
	#pragma warning disable SA1516
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
