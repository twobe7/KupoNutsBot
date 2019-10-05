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

	public class UseItemPage : DialogPage
	{
		private readonly Consumable item;
		private readonly Status status;
		private readonly PageBase previousPage;

		public UseItemPage(Consumable item, Status status, PageBase prevPage)
		{
			this.item = item;
			this.status = status;
			this.previousPage = prevPage;
		}

		protected override async Task Cancel()
		{
			await this.Renderer.SetPage(this.previousPage);
		}

		protected override async Task Confirm()
		{
			this.status.GetOrAddItems(this.item.Id).Count--;
			await RPGService.SaveStatus(this.status);

			// TODO: select target...
			string msg = await this.item.Use(this.Renderer.User, this.Renderer.User);

			await this.Renderer.SetPage(new GenericDialogPage(this.item.Name, msg, this.previousPage));
		}

		protected override Task<string> GetContent()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Use ");
			builder.Append(this.item.Name);
			builder.AppendLine("?");

			return Task.FromResult(builder.ToString());
		}

		protected override string GetTitle()
		{
			return "Confirm";
		}
	}
}
