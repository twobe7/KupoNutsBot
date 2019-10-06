// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.RPG
{
	using System.Collections.Generic;

	public class Status : EntryBase
	{
		public int Level = 1;
		public int Nuts = 10;

		public List<ItemStack> Inventory = new List<ItemStack>();

		public ItemStack GetOrAddItems(string id)
		{
			foreach (ItemStack stack in this.Inventory)
			{
				if (stack.ItemId == id)
				{
					return stack;
				}
			}

			ItemStack newStack = new ItemStack();
			newStack.ItemId = id;
			newStack.Count = 0;
			this.Inventory.Add(newStack);
			return newStack;
		}

		public class ItemStack
		{
			public string ItemId = string.Empty;
			public int Count = 0;
		}
	}
}
