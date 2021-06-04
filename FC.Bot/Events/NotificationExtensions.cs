// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Events;
	using FC.Utils;
	using NodaTime;

	public static class NotificationExtensions
	{
		public static async Task Post(this Event.Instance self, Event evt)
		{
			SocketTextChannel? channel = evt.GetChannel();
			if (channel is null)
				return;

			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = evt.Color.ToDiscordColor();
			builder.Title = evt.Name;
			builder.Description = evt.Description;
			builder.ImageUrl = evt.Image;

			/*
			 * Due to a bug on Android, we cannot use the Timestamp field for dates in the future.
			 * If discord ever fixes this, we should use timestamps as they automatically adjust to the clients
			 * time zone.
			 * https://trello.com/c/RO4zrt25
			 */
			/*StringBuilder footerBuilder = new StringBuilder();
			footerBuilder.Append(TimeUtils.GetDurationString(evt.Duration));
			footerBuilder.Append(" ");
			footerBuilder.Append(evt.GetRepeatsString());

			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = footerBuilder.ToString();
			builder.Timestamp = evt.NextOccurance();*/

			StringBuilder descBuilder = new StringBuilder();

			// Starts in: 1 hour 45 minutes
			descBuilder.Append("__");
			descBuilder.Append(evt.GetWhenString());
			descBuilder.AppendLine("__");
			descBuilder.AppendLine();

			Occurance? occurance = evt.GetNextOccurance();
			if (occurance != null)
			{
				descBuilder.AppendLine(occurance.GetDisplayString());
				descBuilder.AppendLine();
			}

			// description
			descBuilder.AppendLine(evt.Description);
			builder.Description = descBuilder.ToString();

			if (evt.StatusType == Event.Statuses.Roles)
			{
				builder.AddStatus(evt, Emotes.Tank);
				builder.AddStatus(evt, Emotes.Healer);
				builder.AddStatus(evt, Emotes.DPS);
			}
			else if (evt.StatusType == Event.Statuses.Attending)
			{
				builder.AddStatus(evt, Emotes.Yes);
				builder.AddStatus(evt, Emotes.Maybe);
				builder.AddStatus(evt, Emotes.No);
			}

			// Notification bell field
			bool shouldBell = true;
			if (occurance != null)
			{
				Duration timeTill = occurance.GetInstant() - TimeUtils.RoundInstant(TimeUtils.Now);
				if (timeTill.TotalMinutes < 60)
				{
					shouldBell = false;
				}
			}

			if (shouldBell)
				builder.AddField(Emotes.Bell.GetString() + " Notify", evt.GetReminderString());

			RestUserMessage? message = await self.GetMessage(evt);
			if (message is null)
			{
				Log.Write("Posting Instance: \"" + evt.Name + "\" (" + evt.Id + ") in channel: \"" + channel.Name + "\" (" + channel.Id + ")", "Bot");

				message = await channel.SendMessageAsync(evt.Message, false, builder.Build());
				self.MessageId = message.Id.ToString();

				List<IEmote> reactions = new List<IEmote>();
				if (evt.StatusType == Event.Statuses.Roles)
				{
					reactions.Add(Emotes.Tank);
					reactions.Add(Emotes.Healer);
					reactions.Add(Emotes.DPS);
				}
				else
				{
					reactions.Add(Emotes.Yes);
					reactions.Add(Emotes.Maybe);
				}

				reactions.Add(Emotes.No);
				reactions.Add(Emotes.Bell);

				await message.AddReactionsAsync(reactions.ToArray());

				EventsService.Instance.Watch(evt);
			}
			else
			{
				Log.Write("Updating Instance: \"" + evt.Name + "\" (" + evt.Id + ") in channel: \"" + channel.Name + "\" (" + channel.Id + ")", "Bot");

				await message.ModifyAsync(x =>
				{
					x.Embed = builder.Build();
					x.Content = evt.Message;
				});
			}
		}

		public static async Task Notify(this Event.Instance self, Event evt)
		{
			if (!string.IsNullOrEmpty(self.NotifyMessageId))
				return;

			SocketTextChannel? channel = evt.GetChannel();
			if (channel is null)
				return;

			Occurance? nextOccurance = evt.GetNextOccurance();
			if (nextOccurance is null)
				return;

			Duration timeTill = nextOccurance.GetInstant() - TimeUtils.RoundInstant(TimeUtils.Now);
			if (timeTill.TotalMinutes > 60)
				return;

			// Do notify
			StringBuilder builder = new StringBuilder();
			builder.Append("Hey! ");
			foreach (Event.Instance.Attendee attendee in self.Attendees)
			{
				if (!attendee.Notify)
					continue;

				builder.Append(attendee.GetMention(evt));
				builder.Append(", ");
			}

			builder.AppendLine();
			builder.Append("The event: [");
			builder.Append(evt.Name);
			builder.Append("](");
			builder.Append("https://discordapp.com/channels/");
			builder.Append(evt.ServerIdStr);
			builder.Append("/");
			builder.Append(evt.ChannelId);
			builder.Append("/");
			builder.Append(self.MessageId);
			builder.Append(" \"");
			builder.Append(evt.Description);
			builder.Append("\") starts in ");
			builder.Append(timeTill.TotalMinutes);
			builder.Append(" minutes!");

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Description = builder.ToString();

			RestUserMessage message = await channel.SendMessageAsync(null, false, embedBuilder.Build());
			self.NotifyMessageId = message.Id.ToString();
		}

		public static async Task Delete(this Event.Instance self, Event evt)
		{
			RestUserMessage? message = await self.GetMessage(evt);

			if (message != null)
				await message.DeleteAsync();

			message = await self.GetNotifyMessage(evt);
			if (message != null)
				await message.DeleteAsync();

			Log.Write("Delete Event Messages: \"" + evt.Name + "\" (" + evt.Id + ")", "Bot");
		}

		public static async Task CheckReactions(this Event.Instance self, Event evt)
		{
			RestUserMessage? message = await self.GetMessage(evt);
			if (message is null)
				return;

			foreach ((IEmote emote, ReactionMetadata data) in message.Reactions)
			{
				(string display, int index) = EventsService.GetStatus(emote);

				if (index < 0)
					continue;

				await self.CheckReactions(evt, message, emote, index);
			}
		}

		public static string GetLink(this Event.Instance self, Event evt)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("[");
			builder.Append(evt.Name);
			builder.Append("](");
			builder.Append("https://discordapp.com/channels/");
			builder.Append(evt.ServerIdStr);
			builder.Append("/");
			builder.Append(evt.ChannelId);
			builder.Append("/");
			builder.Append(self.MessageId);

			builder.Append(" \"");
			builder.Append(evt.Description);
			builder.Append("\"");

			builder.Append(")");

			return builder.ToString();
		}

		public static async Task<RestUserMessage?> GetMessage(this Event.Instance self, Event evt)
		{
			if (self.MessageId == null)
				return null;

			SocketTextChannel? channel = evt.GetChannel();
			if (channel is null)
				return null;

			IMessage message = await channel.GetMessageAsync(ulong.Parse(self.MessageId));
			if (message is null)
				return null;

			if (message is RestUserMessage userMessage)
				return userMessage;

			throw new Exception("Message: \"" + self.MessageId + "\" is not a user message.");
		}

		public static async Task<RestUserMessage?> GetNotifyMessage(this Event.Instance self, Event evt)
		{
			if (self.NotifyMessageId == null)
				return null;

			SocketTextChannel? channel = evt.GetChannel();
			if (channel is null)
				return null;

			IMessage message = await channel.GetMessageAsync(ulong.Parse(self.NotifyMessageId));
			if (message is null)
				return null;

			if (message is RestUserMessage userMessage)
				return userMessage;

			throw new Exception("Message: \"" + self.NotifyMessageId + "\" is not a user message.");
		}

		private static void AddStatus(this EmbedBuilder builder, Event evt, IEmote emote)
		{
			(string display, int index) = EventsService.GetStatus(emote);

			if (index < 0)
				return;

			int count = 0;
			string attending = evt.GetAttendeeString(index, out count);
			builder.AddField(emote.GetString() + " " + display + " (" + count + ")", attending, true);
		}

		private static async Task CheckReactions(this Event.Instance self, Event evt, RestUserMessage message, IEmote emote, int index)
		{
			IEnumerable<IUser> checkedUsers = await message.GetReactionUsersAsync(emote, 99).FlattenAsync();

			if (!checkedUsers.Contains(Program.DiscordClient.CurrentUser))
				await message.AddReactionAsync(emote);

			foreach (IUser user in checkedUsers)
			{
				if (user.Id == Program.DiscordClient.CurrentUser.Id)
					continue;

				evt.SetAttendeeStatus(user.Id, index);

				SocketUser socketUser = Program.DiscordClient.GetUser(user.Id);
				await message.RemoveReactionAsync(emote, socketUser);
			}
		}

		private static bool Contains(this IEnumerable<IUser> self, IUser item)
		{
			foreach (IUser user in self)
			{
				if (user.Id == item.Id)
				{
					return true;
				}
			}

			return false;
		}
	}
}
