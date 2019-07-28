// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNutsBot.Events;
	using KupoNutsBot.Utils;

	public class ReminderService : ServiceBase
	{
		private static ReminderService instance;

		private static IEmote emoteCancel = Emote.Parse("<:No:604942582589423618>");
		private static IEmote emote15mins = Emote.Parse("<:15mins:604947121786978308>");
		private static IEmote emote30mins = Emote.Parse("<:30mins:604947121921064960>");
		private static IEmote emote1hour = Emote.Parse("<:1hour:604947121778720815>");
		private static IEmote emote1day = Emote.Parse("<:1day:604947121380261899>");

		private Dictionary<ulong, PendingReminder> messageLookup = new Dictionary<ulong, PendingReminder>();

		public static void SetReminder(Event.Attendee attendee, string message)
		{
			_ = Task.Run(async () => { await instance.ConfirmReminder(attendee, message); });
		}

		public override Task Initialize()
		{
			instance = this;

			Program.DiscordClient.ReactionAdded += this.ReactionAdded;

			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;

			return Task.CompletedTask;
		}

		private static TimeSpan? GetDelaytime(IEmote emote)
		{
			if (emote.Name == emote15mins.Name)
				return new TimeSpan(0, 15, 0);

			if (emote.Name == emote30mins.Name)
				return new TimeSpan(0, 30, 0);

			if (emote.Name == emote1hour.Name)
				return new TimeSpan(1, 0, 0);

			if (emote.Name == emote1day.Name)
				return new TimeSpan(1, 0, 0, 0);

			return null;
		}

		private async Task ConfirmReminder(Event.Attendee attendee, string messageString)
		{
			SocketUser user = Program.DiscordClient.GetUser(attendee.UserId);

			string remindString = null;
			if (attendee.RemindTime != null)
				remindString = "\nYou're already set to recieve a reminder " + TimeUtils.GetDurationString((TimeSpan)attendee.RemindTime) + " before the event";

			IUserMessage message = await user.SendMessageAsync(messageString + remindString + "\nHow much of a heads up would you like?");

			this.messageLookup.Add(message.Id, new PendingReminder(attendee));

			List<IEmote> reactions = new List<IEmote>();

			reactions.Add(emoteCancel);
			reactions.Add(emote15mins);
			reactions.Add(emote30mins);
			reactions.Add(emote1hour);
			reactions.Add(emote1day);

			await message.AddReactionsAsync(reactions.ToArray());
		}

		private Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return Task.CompletedTask;

				_ = Task.Run(async () =>
				{
					IUserMessage initialMessage = await message.GetOrDownloadAsync();
					await this.HandleResponse(initialMessage, reaction.Emote, reaction.UserId);
				});
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}

			return Task.CompletedTask;
		}

		private async Task HandleResponse(IUserMessage message, IEmote emote, ulong userId)
		{
			if (!this.messageLookup.ContainsKey(message.Id))
				return;

			PendingReminder reminder = this.messageLookup[message.Id];
			TimeSpan? time = GetDelaytime(emote);
			reminder.SetDelay(time);

			SocketUser user = Program.DiscordClient.GetUser(userId);
			IUserMessage replyMessage;
			if (time == null)
			{
				replyMessage = await user.SendMessageAsync("Got it, I wont remind you.");
			}
			else
			{
				replyMessage = await user.SendMessageAsync("Got it, I'll let you know " + TimeUtils.GetDurationString((TimeSpan)time) + "before the event starts!\n\n(this message will self-destruct in 5 seconds)");
			}

			await Task.Delay(5000);

			await message.DeleteAsync();
			await replyMessage.DeleteAsync();

			this.messageLookup.Remove(message.Id);
		}

		public class PendingReminder
		{
			public ulong UserId;
			public Event.Attendee Attendee;

			public PendingReminder(Event.Attendee attendee)
			{
				this.UserId = attendee.UserId;
				this.Attendee = attendee;
			}

			public void SetDelay(TimeSpan? time)
			{
				this.Attendee.RemindTime = time;
			}
		}
	}
}
