// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG.StorePages
{
	using System.Threading.Tasks;
	using KupoNuts.Bot.Pages;

	public class PurchaseCompletePage : DialogPage
	{
		private readonly PageBase nextPage;

		public PurchaseCompletePage(PageBase nextPage)
		{
			this.nextPage = nextPage;
		}

		protected override async Task Confirm()
		{
			await this.Renderer.SetPage(this.nextPage);
		}

		protected override async Task Cancel()
		{
			await this.Renderer.SetPage(this.nextPage);
		}

		protected override string GetTitle()
		{
			return "Purchase Complete";
		}

		protected override Task<string> GetContent()
		{
			return Task.FromResult("Your new item is now in your inventory!");
		}
	}
}
