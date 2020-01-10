// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.RPG
{
	public class Item : EntryBase
	{
		public string? Name { get; set; }

		public string? Description { get; set; }

		public int? Cost { get; set; }

		public string? Macro { get; set; }
	}
}
