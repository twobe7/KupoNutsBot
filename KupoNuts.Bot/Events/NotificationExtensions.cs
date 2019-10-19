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
	using KupoNuts.Events;
	using KupoNuts.Utils;
	using NodaTime;

	public static class NotificationExtensions
	{
		public static async Task Post(this Event.Notification self, Event evt)
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
			 * Due to a bug on Android, we cannot use the Timestamp field for dates in teh future.
			 * If discord ever fixes this, we should use timestamps as thay automatically adjust to the clients
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

			Event.Occurance? occurance = evt.GetNextOccurance();
			if (occurance != null)
			{
				descBuilder.AppendLine(occurance.GetDisplayString());
				descBuilder.AppendLine();
			}

			// desc
			descBuilder.AppendLine(evt.Description);
			builder.Description = descBuilder.ToString();

			if (evt.Statuses != null)
			{
				for (int i = 0; i < evt.Statuses.Count; i++)
				{
					Event.Status status = evt.Statuses[i];

					if (string.IsNullOrEmpty(status.Display))
						continue;

					int count = 0;
					string attending = evt.GetAttendeeString(i, out count);

					builder.AddField(status.Display + " (" + count + ")", attending, true);
				}
			}

			RestUserMessage? message = await self.GetMessage(evt);
			if (message is null)
			{
				Log.Write("Posting notification: \"" + evt.Name + "\" (" + evt.Id + ") in channel: \"" + channel.Name + "\" (" + channel.Id + ")", "Bot");

				message = await channel.SendMessageAsync(evt.Message, false, builder.Build());
				self.MessageId = message.Id.ToString();

				List<IEmote> reactions = new List<IEmote>();

				if (!string.IsNullOrEmpty(evt.RemindMeEmote))
					reactions.Add(evt.GetRemindMeEmote());

				if (evt.Statuses != null)
				{
					foreach (Event.Status status in evt.Statuses)
					{
						reactions.Add(status.GetEmote());
					}
				}

				await message.AddReactionsAsync(reactions.ToArray());

				EventsService.Instance.Watch(evt);
			}
			else
			{
				Log.Write("Updating notification: \"" + evt.Name + "\" (" + evt.Id + ") in channel: \"" + channel.Name + "\" (" + channel.Id + ")", "Bot");

				await message.ModifyAsync(x =>
				{
					x.Embed = builder.Build();
					x.Content = evt.Message;
				});
			}
		}

		public static async Task Delete(this Event.Notification self, Event evt)
		{
			RestUserMessage? message = await self.GetMessage(evt);
			if (message is null)
				return;

			await message.DeleteAsync();
		}

		public static async Task CheckReactions(this Event.Notification self, Event evt)
		{
			RestUserMessage? message = await self.GetMessage(evt);
			if (message is null)
				return;

			if (evt.Statuses != null)
			{
				for (int i = 0; i < evt.Statuses.Count; i++)
				{
					Event.Status status = evt.Statuses[i];
					await self.CheckReactions(evt, message, status, i);
				}
			}
		}

		public static string GetLink(this Event.Notification self, Event evt)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("[");
			builder.Append(evt.Name);
			builder.Append("](");
			builder.Append("https://discordapp.com/channels/");
			builder.Append(evt.ServerId);
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

		public static async Task<RestUserMessage?> GetMessage(this Event.Notification self, Event evt)
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

		private static async Task CheckReactions(this Event.Notification self, Event evt, RestUserMessage message, Event.Status status, int statusIndex)
		{
			IEmote emote = status.GetEmote();
			IEnumerable<IUser> checkedUsers = await message.GetReactionUsersAsync(emote, 99).FlattenAsync();

			if (!checkedUsers.Contains(Program.DiscordClient.CurrentUser))
				await message.AddReactionAsync(emote);

			foreach (IUser user in checkedUsers)
			{
				if (user.Id == Program.DiscordClient.CurrentUser.Id)
					continue;

				evt.SetAttendeeStatus(user.Id, statusIndex);

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
