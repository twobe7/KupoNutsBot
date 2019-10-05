// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;

	public class Store
	{
		public static IEmote CloseEmote = Emote.Parse("<:No:604942582589423618>");

		private static List<Store> stores = new List<Store>();

		private ISocketMessageChannel channel;
		private IGuildUser user;
		private RestUserMessage? message;

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

		public void Open()
		{
			Program.DiscordClient.ReactionAdded += this.OnReactionAdded;

			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = "Kupo Nut Shop - " + this.user.GetName();

			this.UpdateStore(builder.Build());
		}

		public void Close()
		{
			Program.DiscordClient.ReactionAdded -= this.OnReactionAdded;

			_ = Task.Run(async () =>
			{
				if (this.message == null)
					return;

				await this.message.ModifyAsync(x =>
				{
					x.Embed = null;
					x.Content = "Thanks for shopping with Kupo Nuts!";
				});

				await this.message.RemoveAllReactionsAsync();
			});
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

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (this.message == null)
				return;

			if (message.Id != this.message.Id)
				return;

			if (reaction.UserId != Program.DiscordClient.CurrentUser.Id)
				await this.message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

			if (reaction.UserId != this.user.Id)
				return;

			if (reaction.Emote.Name == CloseEmote.Name)
			{
				this.Close();
			}
		}

		private void UpdateStore(Embed embed)
		{
			_ = Task.Run(async () =>
			{
				await this.UpdateStoreAsync(embed);
			});
		}

		private async Task UpdateStoreAsync(Embed embed)
		{
			if (this.message == null)
			{
				this.message = await this.channel.SendMessageAsync(null, false, embed);
				await this.message.AddReactionsAsync(new IEmote[] { CloseEmote });
			}
			else
			{
				await this.message.ModifyAsync(x =>
				{
					x.Embed = embed;
				});
			}
		}
	}
}
