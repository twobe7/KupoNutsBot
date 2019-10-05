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

			new StatusConsumable(0, "Level Up x10", "Increases your character level by 10", 100, (user, status) =>
			{
				status.Level += 10;
				return Task.FromResult(user.GetName() + " is now level " + status.Level);
			}),
		};
	}
}
