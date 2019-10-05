// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG.StorePages
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using KupoNuts.Bot.Pages;
	using KupoNuts.Bot.RPG.Items;
	using KupoNuts.RPG;

	public class PurchasePage : DialogPage
	{
		private readonly ItemBase item;
		private readonly Status status;
		private readonly PageBase previousPage;

		public PurchasePage(ItemBase item, Status status, PageBase previousPage)
		{
			this.item = item;
			this.status = status;
			this.previousPage = previousPage;
		}

		protected override async Task Confirm()
		{
			if (this.item.Cost > this.status.Nuts)
			{
				await this.Cancel();
			}
			else
			{
				this.status.Nuts -= this.item.Cost;
				this.status.Inventory.Add(this.item.Id);
				await RPGService.SaveStatus(this.status);
				await this.Renderer.SetPage(new PurchaseCompletePage(this.previousPage));
			}
		}

		protected override async Task Cancel()
		{
			await this.Renderer.SetPage(this.previousPage);
		}

		protected override string GetTitle()
		{
			return "Purchase " + this.item.Name;
		}

		protected override Task<string> GetContent()
		{
			StringBuilder builder = new StringBuilder();

			if (this.item.Cost > this.status.Nuts)
			{
				builder.Append("you dont have enough ");
				builder.AppendLine(RPGService.NutEmoteStr);
				builder.AppendLine();
				builder.AppendLine("Confirm?");
			}
			else
			{
				builder.Append("This will cost ");
				builder.Append(this.item.Cost);
				builder.AppendLine(RPGService.NutEmoteStr);
				builder.AppendLine();
				builder.AppendLine("Confirm?");
			}

			return Task.FromResult(builder.ToString());
		}
	}
}
