// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Pages;
	using KupoNuts.Bot.RPG.StorePages;
	using KupoNuts.RPG;

	public class Store
	{
		private static List<Store> stores = new List<Store>();

		private ISocketMessageChannel channel;
		private IGuildUser user;

		private PageRenderer pageRenderer = new PageRenderer();

		public Store(ISocketMessageChannel channel, IGuildUser user)
		{
			this.channel = channel;
			this.user = user;
		}

		public static void BeginStore(ISocketMessageChannel channel, IGuildUser user)
		{
			Store? store = GetStore(user);
			if (store != null)
				store.Close();

			store = new Store(channel, user);
			store.Open();
		}

		private static Store? GetStore(IGuildUser user)
		{
			foreach (Store store in stores)
			{
				if (store.user == user)
				{
					return store;
				}
			}

			return null;
		}

		private void Open()
		{
			_ = Task.Run(async () =>
			{
				Status status = await RPGService.GetStatus(this.user);

				EmbedBuilder builder = new EmbedBuilder();
				builder.Description = "Thanks for shopping with Kupo Nuts!";
				builder.ThumbnailUrl = "https://www.kuponutbrigade.com/wp-content/uploads/2019/10/bye.png";

				await this.pageRenderer.Create(this.channel, this.user, builder.Build());
				await this.pageRenderer.SetPage(new ItemsList(status));
			});
		}

		private void Close()
		{
			_ = Task.Run(this.pageRenderer.Destroy);
			stores.Remove(this);
		}
	}
}
