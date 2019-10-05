// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Pages
{
	using System.Threading.Tasks;

	public abstract class DialogPage : PageBase
	{
		private readonly PageBase previousPage;

		public DialogPage(PageBase previousPage)
		{
			this.previousPage = previousPage;
		}

		public override async Task Navigate(Navigation nav)
		{
			if (nav == Navigation.Yes)
			{
				await this.Confirm();
			}
			else if (nav == Navigation.No)
			{
				await this.Cancel();
			}
		}

		public abstract Task Confirm();

		protected async Task Cancel()
		{
			await this.Renderer.SetPage(this.previousPage);
		}
	}
}
