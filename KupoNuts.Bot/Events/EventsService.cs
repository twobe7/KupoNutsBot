// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using KupoNuts.Events;

	public class EventsService : ServiceBase
	{
		private Dictionary<ulong, Event> messageLookup = new Dictionary<ulong, Event>();

		public static EventsService Instance
		{
			get;
			private set;
		}

		public override async Task Initialize()
		{
			Instance = this;

			CommandsService.BindCommand("notify", this.Notify, Permissions.Administrators, "Posts notifications for all events, regardless of schedule.");

			foreach (Event evt in Database.Load().Events)
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
				foreach (Event evt in Database.Load().Events)
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

			if (!string.IsNullOrEmpty(evt.RemindMeEmote))
			{
				if (reaction.Emote.Name == evt.GetRemindMeEmote().Name)
				{
					ReminderService.SetReminder(attendee, "I'll remind you before the event: \"" + evt.Name + "\".");
				}
			}
			else
			{
				for (int i = 0; i < evt.Statuses.Count; i++)
				{
					Event.Status status = evt.Statuses[i];

					if (reaction.Emote.Name == status.GetEmote().Name)
					{
						attendee.Status = i;
					}
				}
			}

			await evt.UpdateNotifications();

			Database.UpdateOrInsert(evt);

			RestUserMessage userMessage = (RestUserMessage)await channel.GetMessageAsync(message.Id);
			SocketUser user = Program.DiscordClient.GetUser(reaction.UserId);
			await userMessage.RemoveReactionAsync(reaction.Emote, user);
		}
	}
}
