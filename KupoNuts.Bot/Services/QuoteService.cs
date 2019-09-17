// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;

	public class QuoteService : ServiceBase
	{
		public override Task Initialize()
		{
			Program.DiscordClient.ReactionAdded += this.OnReactionAdded;
			return base.Initialize();
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.OnReactionAdded;
			return base.Shutdown();
		}

		private Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			Log.Write(reaction.Emote.Name);

			return Task.CompletedTask;
		}
	}
}
