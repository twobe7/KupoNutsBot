// This document is intended for use by Kupo Nut Brigade developers.

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
