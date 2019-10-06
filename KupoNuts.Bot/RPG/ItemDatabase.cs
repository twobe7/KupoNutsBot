// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using KupoNuts.Bot.RPG.Items;

	public static class ItemDatabase
	{
		public static List<ItemBase> Items = new List<ItemBase>()
		{
			new StatusConsumable(0, "Level Up", "Increases your character level by 1", 10, (user, status) =>
			{
				status.Level++;
				return Task.FromResult(user.GetName() + " is now level " + status.Level);
			}),

			new StatusConsumable(1, "Level Up x10", "Increases your character level by 10", 100, (user, status) =>
			{
				status.Level += 10;
				return Task.FromResult(user.GetName() + " is now level " + status.Level);
			}),

			new MacroItem(2, "Wet fish", "a slippery salmon", 1, "<me> hits <t> in the face with a wet fish!"),
		};

		public static ItemBase FindItem(string itemName)
		{
			foreach (ItemBase item in Items)
			{
				if (item.Name.ToLower() == itemName.ToLower())
				{
					return item;
				}
			}

			throw new Exception("Unknown item: " + itemName);
		}

		public static ItemBase GetItem(int id)
		{
			foreach (ItemBase item in Items)
			{
				if (item.Id == id)
				{
					return item;
				}
			}

			throw new Exception("Unknown item: " + id);
		}
	}
}
