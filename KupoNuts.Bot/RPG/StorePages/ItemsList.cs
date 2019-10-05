// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG.StorePages
{
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using KupoNuts.Bot.Pages;
	using KupoNuts.Bot.RPG.Items;
	using KupoNuts.RPG;

	public class ItemsList : ListPageBase
	{
		private readonly Status status;

		public ItemsList(Status status)
		{
			this.status = status;
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

		protected override string GetTitle()
		{
			return "Items";
		}

		protected override int GetItemCount()
		{
			return ItemDatabase.Items.Count;
		}

		protected override string GetItemName(int index)
		{
			ItemBase item = ItemDatabase.Items[index];

			int pad = 50 - Utils.Characters.GetWidth(item.Name);

			StringBuilder builder = new StringBuilder();
			builder.Append(item.Name);
			builder.Append(" ");

			for (int i = 0; i < pad; i++)
				builder.Append('.');

			builder.Append(" ");
			builder.Append(item.Cost);

			return builder.ToString();
		}

		protected override string GetItemDesc(int index)
		{
			return ItemDatabase.Items[index].Description;
		}

		protected override async Task<string> GetContent()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("You have ");
			builder.Append(this.status.Nuts.ToString());
			builder.AppendLine(" Kupo Nuts");
			builder.AppendLine();

			builder.Append(await base.GetContent());
			return builder.ToString();
		}

		protected override async Task Select(int index)
		{
			if (this.Renderer == null)
				return;

			ItemBase item = ItemDatabase.Items[index];
			await this.Renderer.SetPage(new PurchasePage(item, this.status, this));
		}
	}
}
