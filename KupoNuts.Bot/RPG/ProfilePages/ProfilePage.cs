// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG.ProfilePages
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using KupoNuts.Bot.Pages;
	using KupoNuts.Bot.RPG.Items;
	using KupoNuts.RPG;

	public class ProfilePage : ListPageBase
	{
		private Status status;
		private List<ItemStack> inventory = new List<ItemStack>();

		public ProfilePage(Status status)
		{
			this.status = status;
		}

		public override async Task Initialize()
		{
			if (this.status.Id == null)
				throw new Exception();

			this.status = await RPGService.GetStatus(this.status.Id);

			this.inventory.Clear();
			foreach (Status.ItemStack itemStack in this.status.Inventory)
			{
				if (itemStack.Count <= 0)
					continue;

				ItemBase item = ItemDatabase.GetItem(itemStack.ItemId);
				this.inventory.Add(new ItemStack(item, itemStack.Count));
			}

			await base.Initialize();
		}

		public override async Task Navigate(Navigation nav)
		{
			if (nav == Navigation.No)
			{
				await this.Renderer.Destroy();
				return;
			}

			await base.Navigate(nav);
		}

		protected override async Task<string> GetContent()
		{
			StringBuilder descBuilder = new StringBuilder();
			descBuilder.Append("Level: ");
			descBuilder.Append(this.status.Level.ToString());
			descBuilder.Append(Utils.Characters.DoubleTab);

			descBuilder.Append(RPGService.NutEmoteStr);
			descBuilder.Append(" ");
			descBuilder.AppendLine(this.status.Nuts.ToString());
			descBuilder.AppendLine();

			descBuilder.AppendLine("__Inventory__");
			descBuilder.Append(await base.GetContent());

			return descBuilder.ToString();
		}

		protected override int GetItemCount()
		{
			return this.inventory.Count;
		}

		protected override string GetItemDesc(int index)
		{
			return this.inventory[index].Item.Description;
		}

		protected override string GetItemName(int index)
		{
			ItemStack stack = this.inventory[index];

			if (stack.Count <= 1)
				return stack.Item.Name;

			return stack.Item.Name + " x" + stack.Count;
		}

		protected override string GetTitle()
		{
			return this.Renderer.User.GetName();
		}

		protected override async Task Select(int index)
		{
			ItemStack stack = this.inventory[index];
			if (stack.Item is Consumable consumable)
			{
				await this.Renderer.SetPage(new UseItemPage(consumable, this.status, this));
			}
		}

		private class ItemStack
		{
			public readonly ItemBase Item;
			public int Count = 0;

			public ItemStack(ItemBase item, int count = 0)
			{
				this.Item = item;
				this.Count = count;
			}
		}
	}
}
