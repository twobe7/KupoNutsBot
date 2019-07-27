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

		public static IEmote EmoteCheck
		{
			get
			{
				return new Emoji(EmojiCheck);
			}
		}

		public static IEmote EmoteCross
		{
			get
			{
				return new Emoji(EmojiCross);
			}
		}

		public static bool IsEmoteCheck(IEmote emote)
		{
			if (emote is Emoji emoji)
			{
				return emoji.Name == EmojiCheck;
			}

			return false;
		}

		public static bool IsEmoteCross(IEmote emote)
		{
			if (emote is Emoji emoji)
			{
				return emoji.Name == EmojiCross;
			}

			return false;
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

				await evt.CheckReactions();
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
			return this.React(message.Id, reaction.Emote, reaction.UserId, false);
		}

		private Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			return this.React(message.Id, reaction.Emote, reaction.UserId, true);
		}

		private async Task React(ulong messageId, IEmote emote, ulong userId, bool added)
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

			if (IsEmoteCross(emote))
			{
				changed = attendee.RespondedNo != added;
				attendee.RespondedNo = added;

				if (attendee.RespondedYes && added)
				{
					await message.RemoveReactionAsync(EmoteCheck, user);
					changed = false;
				}
			}

			if (IsEmoteCheck(emote))
			{
				changed = attendee.RespondedYes != added;
				attendee.RespondedYes = added;

				if (attendee.RespondedNo && added)
				{
					await message.RemoveReactionAsync(EmoteCross, user);
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
