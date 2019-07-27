// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNutsBot.Extensions;
	using KupoNutsBot.Utils;

	[Serializable]
	public class Event
	{
		public ulong ChannelId;
		public string Name;
		public string Description;
		public string Image;
		public Colors Color;
		public DateTime DateTime;
		public Days Repeats;
		public TimeSpan Duration;

		public List<Status> Statuses = new List<Status>();
		public List<Attendee> Attendees = new List<Attendee>();
		public List<Notification> Notifications = new List<Notification>();

		[Flags]
		public enum Days
		{
			None = 0,

			Monday = 1,
			Tuesday = 2,
			Wednesday = 4,
			Thursday = 8,
			Friday = 16,
			Saturday = 32,
			Sunday = 64,
		}

		public enum Colors
		{
			Default,
			DarkerGrey,
			DarkGrey,
			LighterGrey,
			DarkRed,
			Red,
			DarkOrange,
			Orange,
			LightOrange,
			Gold,
			LightGrey,
			Magenta,
			DarkPurple,
			Purple,
			DarkBlue,
			Blue,
			DarkGreen,
			Green,
			DarkTeal,
			Teal,
			DarkMagenta,
		}

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
			foreach (Notification notify in this.Notifications)
			{
				await notify.CheckReactions(this);
			}

			await this.UpdateNotifications();
		}

		protected string GetRepeatsString()
		{
			if (this.Repeats == Days.None)
				return null;

			StringBuilder builder = new StringBuilder();
			builder.Append("Every ");

			int count = 0;
			foreach (Days day in Enum.GetValues(typeof(Days)))
			{
				if (FlagsUtils.IsSet(this.Repeats, day))
				{
					if (count > 0)
						builder.Append(", ");

					count++;
					builder.Append(day);
				}
			}

			if (count == 7)
			{
				builder.Clear();
				builder.Append("Every day");
			}

			return builder.ToString();
		}

		protected string GetAttendeeString(int statusIndex, out int count)
		{
			StringBuilder builder = new StringBuilder();

			count = 0;
			foreach (Attendee attendee in this.Attendees)
			{
				if (attendee.Status == statusIndex)
				{
					count++;
					builder.AppendLine(attendee.GetMention());
				}
			}

			if (count <= 0)
				builder.Append("No one yet");

			if (count > 8)
			{
				builder.Clear();
				builder.Append(count + " people");
			}

			return builder.ToString();
		}

		protected Color GetColor()
		{
			switch (this.Color)
			{
				case Colors.Default: return Discord.Color.Default;
				case Colors.DarkerGrey: return Discord.Color.DarkerGrey;
				case Colors.DarkGrey: return Discord.Color.DarkGrey;
				case Colors.LighterGrey: return Discord.Color.LighterGrey;
				case Colors.DarkRed: return Discord.Color.DarkRed;
				case Colors.Red: return Discord.Color.Red;
				case Colors.DarkOrange: return Discord.Color.DarkOrange;
				case Colors.Orange: return Discord.Color.Orange;
				case Colors.LightOrange: return Discord.Color.LightOrange;
				case Colors.Gold: return Discord.Color.Gold;
				case Colors.LightGrey: return Discord.Color.LightGrey;
				case Colors.Magenta: return Discord.Color.Magenta;
				case Colors.DarkPurple: return Discord.Color.DarkPurple;
				case Colors.Purple: return Discord.Color.Purple;
				case Colors.DarkBlue: return Discord.Color.DarkBlue;
				case Colors.Blue: return Discord.Color.Blue;
				case Colors.DarkGreen: return Discord.Color.DarkGreen;
				case Colors.Green: return Discord.Color.Green;
				case Colors.DarkTeal: return Discord.Color.DarkTeal;
				case Colors.Teal: return Discord.Color.Teal;
				case Colors.DarkMagenta: return Discord.Color.DarkMagenta;
			}

			throw new Exception("Unknown discord color: " + this.Color);
		}

		protected DateTimeOffset NextOccurance()
		{
			return new DateTimeOffset(this.DateTime);
		}

		public class Status
		{
			public string EmoteString;
			public string Display;

			public Status()
			{
			}

			public Status(string emote, string display = null)
			{
				this.EmoteString = emote;
				this.Display = display;
			}

			public IEmote GetEmote()
			{
				return Emote.Parse(this.EmoteString);
			}
		}

		public class Attendee
		{
			public ulong UserId;
			public int Status;

			public string GetMention()
			{
				SocketUser user = Program.DiscordClient.GetUser(this.UserId);
				return user.Mention;
			}
		}

		[Serializable]
		public class Notification
		{
			public ulong MessageId;

			public async Task Post(Event evt)
			{
				EmbedBuilder builder = new EmbedBuilder();
				builder.Color = evt.GetColor();
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

				StringBuilder timeBuilder = new StringBuilder();
				timeBuilder.Append(TimeUtils.GetDurationString(evt.Duration));
				timeBuilder.Append(" ");

				string repeat = evt.GetRepeatsString();
				if (repeat != null)
				{
					timeBuilder.Append(repeat);
					timeBuilder.AppendLine(" at ");
					timeBuilder.Append(TimeUtils.GetTimeString(evt.DateTime));
				}
				else
				{
					timeBuilder.Append(" on ");
					timeBuilder.Append(TimeUtils.GetDateTimeString(evt.DateTime));
				}

				builder.AddField("When", timeBuilder.ToString(), false);

				for (int i = 0; i < evt.Statuses.Count; i++)
				{
					Status status = evt.Statuses[i];

					if (string.IsNullOrEmpty(status.Display))
						continue;

					int count = 0;
					string attending = evt.GetAttendeeString(i, out count);

					builder.AddField(status.Display + " (" + count + ")", attending, true);
				}

				SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(evt.ChannelId);

				RestUserMessage message;
				if (this.MessageId == 0)
				{
					message = await channel.SendMessageAsync(null, false, builder.Build());
					this.MessageId = message.Id;

					foreach (Status status in evt.Statuses)
					{
						await message.AddReactionAsync(status.GetEmote());
						await message.AddReactionAsync(status.GetEmote());
						await message.AddReactionAsync(status.GetEmote());
					}

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

				for (int i = 0; i < evt.Statuses.Count; i++)
				{
					Status status = evt.Statuses[i];
					await this.CheckReactions(evt, channel, message, status, i);
				}
			}

			private async Task CheckReactions(Event evt, SocketTextChannel channel, RestUserMessage message, Status status, int statusIndex)
			{
				IEmote emote = status.GetEmote();
				IEnumerable<IUser> checkedUsers = await message.GetReactionUsersAsync(emote, 99).FlattenAsync();

				if (!checkedUsers.Contains(Program.DiscordClient.CurrentUser))
					await message.AddReactionAsync(emote);

				foreach (IUser user in checkedUsers)
				{
					if (user.Id == Program.DiscordClient.CurrentUser.Id)
						continue;

					Attendee attendee = evt.GetAttendee(user.Id);
					attendee.Status = statusIndex;

					SocketUser socketUser = Program.DiscordClient.GetUser(user.Id);
					await message.RemoveReactionAsync(emote, socketUser);
				}
			}
		}
	}
}
