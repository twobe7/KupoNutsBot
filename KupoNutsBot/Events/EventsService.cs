// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNutsBot.Services;

	public class EventsService : ServiceBase
	{
		public const string EmojiCheck = "\u2705";
		public const string EmojiCross = "\u274C";

		private Dictionary<ulong, Event> messageLookup = new Dictionary<ulong, Event>();

		public static EventsService Instance
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

		public override async Task Initialize()
		{
			Instance = this;

			CommandsService.BindCommand("notify", this.Notify);

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
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("notify");

			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;

			return Task.CompletedTask;
		}

		public void Watch(ulong messageId, Event evt)
		{
			if (this.messageLookup.ContainsKey(messageId))
				return;

			this.messageLookup.Add(messageId, evt);
		}

		private async Task Notify(string[] args, SocketMessage message)
		{
			if (args.Length <= 0)
			{
				foreach (Event evt in Database.Instance.Events)
				{
					await evt.Post();
				}
			}
			else
			{
				await message.Channel.SendMessageAsync("I'm sorry, I cant notify specific events yet.");
			}
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!this.messageLookup.ContainsKey(message.Id))
				return;

			// dont mark yourself as attending!
			if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
				return;

			Event evt = this.messageLookup[message.Id];
			Event.Attendee attendee = evt.GetAttendee(reaction.UserId);

			if (IsEmoteCheck(reaction.Emote))
			{
				attendee.Status = Event.Attendee.Statuses.Attending;
			}
			else if (IsEmoteCross(reaction.Emote))
			{
				attendee.Status = Event.Attendee.Statuses.NotAttending;
			}

			Database.Instance.Save();
			await evt.UpdateNotifications();

			RestUserMessage userMessage = (RestUserMessage)await channel.GetMessageAsync(message.Id);
			SocketUser user = Program.DiscordClient.GetUser(reaction.UserId);
			await userMessage.RemoveReactionAsync(reaction.Emote, user);
		}
	}
}
