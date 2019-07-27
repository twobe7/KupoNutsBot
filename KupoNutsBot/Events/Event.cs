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

	[Serializable]
	public class Event
	{
		public ulong ChannelId;
		public string Name;

		public List<Attendee> Attendees = new List<Attendee>();
		public List<Notification> Notifications = new List<Notification>();

		public async Task Post()
		{
			Notification notify = new Notification();
			await notify.Post(this);
			this.Notifications.Add(notify);
			Database.Instance.Save();
		}

		public async Task UpdateNotifications()
		{
			foreach (Notification notify in this.Notifications)
			{
				await notify.Post(this);
			}
		}

		public Attendee GetAttendee(ulong userId)
		{
			foreach (Attendee attendee in this.Attendees)
			{
				if (attendee.UserId == userId)
				{
					return attendee;
				}
			}

			Attendee newAttendee = new Attendee();
			newAttendee.UserId = userId;
			this.Attendees.Add(newAttendee);
			return newAttendee;
		}

		public async Task CheckReactions()
		{
			this.Attendees.Clear();
			foreach (Notification notify in this.Notifications)
			{
				await notify.CheckReactions(this);
			}

			await this.UpdateNotifications();
		}

		public class Attendee
		{
			public ulong UserId;
			public bool RespondedYes = false;
			public bool RespondedNo = false;

			public string Mention
			{
				get
				{
					SocketUser user = Program.DiscordClient.GetUser(this.UserId);
					return user.Mention;
				}
			}
		}

		[Serializable]
		public class Notification
		{
			public ulong MessageId;

			public async Task Post(Event evt)
			{
				EmbedBuilder builder = new EmbedBuilder();
				builder.Color = Color.DarkRed;
				builder.Title = evt.Name;
				builder.Description = "A description goes here";

				builder.ImageUrl = "https://www.kuponutbrigade.com/wp-content/uploads/2019/03/cropped-BG2-1.jpg";

				StringBuilder goingBuilder = new StringBuilder();
				StringBuilder notGoingBuilder = new StringBuilder();

				int goingCount = 0;
				int notGoingCount = 0;
				foreach (Attendee attendee in evt.Attendees)
				{
					if (attendee.RespondedYes)
					{
						goingCount++;
						goingBuilder.AppendLine(attendee.Mention);
					}

					if (attendee.RespondedNo)
					{
						notGoingCount++;
						notGoingBuilder.AppendLine(attendee.Mention);
					}
				}

				if (goingCount <= 0)
					goingBuilder.Append("No one yet");

				if (notGoingCount <= 0)
					notGoingBuilder.Append("No one yet");

				if (goingCount > 8)
				{
					goingBuilder.Clear();
					goingBuilder.Append(goingCount + " people");
				}

				if (notGoingCount > 8)
				{
					notGoingBuilder.Clear();
					notGoingBuilder.Append(notGoingCount + " people");
				}

				builder.AddField("Date", "Jul 27 at 8:00 PM AEST", true);
				builder.AddField("Duration", "2 Hours", true);
				builder.AddField("Going", goingBuilder.ToString(), true);
				builder.AddField("Not Going", notGoingBuilder.ToString(), true);

				SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(evt.ChannelId);

				RestUserMessage message;
				if (this.MessageId == 0)
				{
					message = await channel.SendMessageAsync(null, false, builder.Build());
					this.MessageId = message.Id;

					await message.AddReactionAsync(new Emoji(EventsService.EmojiCheck));
					await message.AddReactionAsync(new Emoji(EventsService.EmojiCross));

					EventsService.Instance.Watch(this.MessageId, evt);
				}
				else
				{
					message = (RestUserMessage)await channel.GetMessageAsync(this.MessageId);
					await message.ModifyAsync(x =>
					{
						x.Embed = builder.Build();
					});
				}
			}

			public async Task CheckReactions(Event evt)
			{
				if (this.MessageId == 0)
					return;

				SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(evt.ChannelId);
				RestUserMessage message = (RestUserMessage)await channel.GetMessageAsync(this.MessageId);

				IEnumerable<IUser> checkedUsers = await message.GetReactionUsersAsync(EventsService.EmoteCheck, 99).FlattenAsync();
				foreach (IUser user in checkedUsers)
				{
					if (user.Id == Program.DiscordClient.CurrentUser.Id)
						continue;

					Attendee attendee = evt.GetAttendee(user.Id);
					if (attendee.RespondedYes)
						continue;

					attendee.RespondedYes = true;
				}

				IEnumerable<IUser> crossedUsers = await message.GetReactionUsersAsync(EventsService.EmoteCross, 99).FlattenAsync();
				foreach (IUser user in crossedUsers)
				{
					if (user.Id == Program.DiscordClient.CurrentUser.Id)
						continue;

					Attendee attendee = evt.GetAttendee(user.Id);
					if (attendee.RespondedNo)
						continue;

					attendee.RespondedNo = true;
				}
			}
		}
	}
}
