// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNutsBot.Data;

	public class Events
	{
		public const string EmojiCheck = "\u2705";
		public const string EmojiCross = "\u274C";

		private Dictionary<ulong, Event> messageLookup = new Dictionary<ulong, Event>();

		public static Events Instance
		{
			get;
			private set;
		}

		public async Task Initialize()
		{
			Instance = this;

			foreach (Event evt in Database.Instance.Events)
			{
				foreach (Event.Notification notification in evt.Notifications)
				{
					if (this.messageLookup.ContainsKey(notification.MessageId))
						continue;

					this.messageLookup.Add(notification.MessageId, evt);
				}

				if (evt.Notifications.Count <= 0)
				{
					await evt.Post();
				}
			}

			Program.DiscordClient.ReactionAdded += this.ReactionAdded;
			Program.DiscordClient.ReactionRemoved += this.ReactionRemoved;
		}

		public void Watch(ulong messageId, Event evt)
		{
			if (this.messageLookup.ContainsKey(messageId))
				return;

			this.messageLookup.Add(messageId, evt);
		}

		private Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (reaction.Emote is Emoji emoji)
			{
				return this.React(message.Id, emoji.Name, reaction.UserId, false);
			}

			return Task.CompletedTask;
		}

		private Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (reaction.Emote is Emoji emoji)
			{
				return this.React(message.Id, emoji.Name, reaction.UserId, true);
			}

			return Task.CompletedTask;
		}

		private async Task React(ulong messageId, string emoji, ulong userId, bool added)
		{
			if (!this.messageLookup.ContainsKey(messageId))
				return;

			// dont mark yourself as attending!
			if (userId == Program.DiscordClient.CurrentUser.Id)
				return;

			Event evt = this.messageLookup[messageId];

			Event.Attendee attendee = evt.GetAttendee(userId);

			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(evt.ChannelId);
			RestUserMessage message = (RestUserMessage)await channel.GetMessageAsync(messageId);
			SocketUser user = Program.DiscordClient.GetUser(userId);

			bool changed = false;

			if (emoji == EmojiCross)
			{
				changed = attendee.RespondedNo != added;
				attendee.RespondedNo = added;

				if (attendee.RespondedYes && added)
				{
					await message.RemoveReactionAsync(new Emoji(EmojiCheck), user);
					changed = false;
				}
			}

			if (emoji == EmojiCheck)
			{
				changed = attendee.RespondedYes != added;
				attendee.RespondedYes = added;

				if (attendee.RespondedNo && added)
				{
					await message.RemoveReactionAsync(new Emoji(EmojiCross), user);
					changed = false;
				}
			}

			if (!changed)
				return;

			Database.Instance.Save();
			await evt.UpdateNotifications();
		}
	}
}
