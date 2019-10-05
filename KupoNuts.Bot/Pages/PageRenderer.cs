// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Timers;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;

	public class PageRenderer
	{
		private static List<PageRenderer> pageRenderers = new List<PageRenderer>();

		private PageBase? currentPage;
		private RestUserMessage? message;
		private ISocketMessageChannel? channel;
		private bool destroyed = false;
		private Timer? timeout;
		private Embed? destroyedEmbed;
		private IGuildUser? user;

		public IGuildUser User
		{
			get
			{
				if (this.user == null)
					throw new Exception("No User in Page Renderer");

				return this.user;
			}
		}

		public async Task Create(ISocketMessageChannel channel, IGuildUser user, Embed? destroyedEmbed = null)
		{
			for (int i = pageRenderers.Count - 1; i >= 0; i--)
			{
				PageRenderer renderer = pageRenderers[i];
				if (renderer.User == user)
				{
					await renderer.Destroy();
				}
			}

			this.channel = channel;
			this.user = user;
			this.destroyedEmbed = destroyedEmbed;
			this.timeout = new Timer(30 * 1000);
			this.timeout.Elapsed += this.OnTimeout;
			this.timeout.Start();

			pageRenderers.Add(this);

			Program.DiscordClient.ReactionAdded += this.OnReactionAdded;
		}

		public async Task SetPage(PageBase page)
		{
			page.Renderer = this;
			this.currentPage = page;
			await this.currentPage.Initialize();
			await this.Render();
		}

		public async Task Destroy()
		{
			if (this.destroyed)
				return;

			this.destroyed = true;
			Program.DiscordClient.ReactionAdded -= this.OnReactionAdded;

			if (this.timeout != null)
				this.timeout.Stop();

			if (this.message != null)
			{
				if (this.destroyedEmbed != null)
				{
					await this.message.RemoveAllReactionsAsync();
					await this.message.ModifyAsync(x =>
					{
						x.Embed = this.destroyedEmbed;
					});
				}
				else
				{
					await this.message.DeleteAsync();
				}
			}

			pageRenderers.Remove(this);
		}

		private async Task Render()
		{
			if (this.destroyed)
				return;

			if (this.channel == null)
				throw new Exception("No Channel in page");

			if (this.currentPage == null)
				throw new Exception("No page in page renderer");

			Embed embed = await this.currentPage.Render();

			if (this.message == null)
			{
				this.message = await this.channel.SendMessageAsync(null, false, embed);
				await this.message.AddReactionsAsync(new IEmote[]
				{
					Navigation.Up.ToEmote(),
					Navigation.Down.ToEmote(),
					////Navigation.Left.ToEmote(),
					////Navigation.Right.ToEmote(),
					Navigation.Yes.ToEmote(),
					Navigation.No.ToEmote(),
				});
			}

			await this.message.ModifyAsync(x =>
			{
				x.Embed = embed;
			});
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				if (this.message == null || this.User == null || this.currentPage == null)
					return;

				if (message.Id != this.message.Id)
					return;

				if (reaction.UserId != Program.DiscordClient.CurrentUser.Id)
					await this.message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

				if (reaction.UserId != this.User.Id)
					return;

				Navigation nav = NavigationExtensions.GetNavigation(reaction.Emote);
				if (nav == Navigation.None)
					return;

				if (this.timeout != null)
				{
					this.timeout.Stop();
					this.timeout.Start();
				}

				await this.currentPage.Navigate(nav);
				await this.Render();
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private void OnTimeout(object sender, ElapsedEventArgs e)
		{
			_ = Task.Run(this.Destroy);
		}
	}
}
