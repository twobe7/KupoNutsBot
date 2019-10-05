// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Pages
{
	using System;
	using System.Threading.Tasks;
	using Discord;

	public abstract class PageBase
	{
		private PageRenderer? renderer;

		public PageRenderer Renderer
		{
			get
			{
				if (this.renderer == null)
					throw new Exception("No renderer on page");

				return this.renderer;
			}

			set
			{
				this.renderer = value;
			}
		}

		public virtual Task Initialize()
		{
			return Task.CompletedTask;
		}

		public abstract Task Navigate(Navigation nav);

		public async Task<Embed> Render()
		{
			if (this.Renderer == null)
				throw new Exception("No Renderer in Page");

			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = this.GetTitle();
			builder.Description = await this.GetContent();

			if (this.Renderer.User != null)
			{
				builder.Footer = new EmbedFooterBuilder();
				builder.Footer.IconUrl = this.Renderer.User.GetAvatarUrl();
				builder.Footer.Text = this.Renderer.User.GetName();
			}

			return builder.Build();
		}

		protected abstract string GetTitle();

		protected abstract Task<string> GetContent();
	}
}
