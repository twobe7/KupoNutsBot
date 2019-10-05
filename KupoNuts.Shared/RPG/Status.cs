// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.RPG
{
	using System.Collections.Generic;

	public class Status : EntryBase
	{
		public int Level = 1;
		public int Nuts = 10;
		public List<int> Inventory = new List<int>();
	}
}
