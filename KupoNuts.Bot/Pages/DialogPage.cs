// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Pages
{
	using System.Threading.Tasks;

	public abstract class DialogPage : PageBase
	{
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

		protected abstract Task Confirm();

		protected abstract Task Cancel();
	}
}
