// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG.Items
{
	public abstract class ItemBase
	{
		public int Id;
		public string Name;
		public string Description;
		public int Cost;

		public ItemBase(int id, string name, string desc, int cost)
		{
			this.Id = id;
			this.Name = name;
			this.Description = desc;
			this.Cost = cost;
		}
	}
}
