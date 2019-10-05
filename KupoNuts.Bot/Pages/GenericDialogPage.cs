// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Pages
{
	using System.Threading.Tasks;

	public class GenericDialogPage : DialogPage
	{
		private readonly string title;
		private readonly string message;
		private readonly PageBase previousPage;

		public GenericDialogPage(string title, string message, PageBase nextPage)
		{
			this.title = title;
			this.message = message;
			this.previousPage = nextPage;
		}

		protected override async Task Cancel()
		{
			await this.Renderer.SetPage(this.previousPage);
		}

		protected override async Task Confirm()
		{
			await this.Renderer.SetPage(this.previousPage);
		}

		protected override Task<string> GetContent()
		{
			return Task.FromResult(this.message);
		}

		protected override string GetTitle()
		{
			return this.title;
		}
	}
}
