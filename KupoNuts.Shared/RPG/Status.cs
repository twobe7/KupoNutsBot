// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.RPG
{
	using System.Collections.Generic;

	public class Status : EntryBase
	{
		public int Level = 1;
		public int Nuts = 10;
		public List<Item> Inventory = new List<Item>();

		public class Item
		{
			public int Id = 0;
			public int Count = 1;
		}
	}
}
