// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Polls
{
	using System;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Utils;

	public static class PollExtensions
	{
		public static async Task<bool> IsValid(this FC.Poll self)
		{
			// Poll is invalid if channel cannot be found
			if (Program.DiscordClient.GetChannel(self.ChannelId) is not SocketTextChannel channel)
				return false;

			if (self.MessageId != 0)
			{
				// Poll is invalid if message has been removed
				if (await channel.GetMessageAsync(self.MessageId) is not RestUserMessage _)
					return false;

				// Poll is invalid if it has been closed for a month
				if (self.Closed() && (self.ClosesInstant - TimeUtils.Now).TotalDays <= -30)
					return false;
			}

			return true;
		}

		public static async Task Close(this FC.Poll self)
		{
			Log.Write("Closing poll: \"" + self.Comment + "\" (" + self.Id + ")", "Bot");

			if (Program.DiscordClient.GetChannel(self.ChannelId) is not SocketTextChannel channel)
				return;

			if (await channel.GetMessageAsync(self.MessageId) is RestUserMessage pollMessage)
				await pollMessage.RemoveAllReactionsAsync();

			self.ClosesInstant = TimeUtils.Now;
		}

		public static Task<Embed> ToEmbed(this FC.Poll self)
		{
			if (Program.DiscordClient.GetChannel(self.ChannelId) is not SocketTextChannel channel)
				throw new Exception("Poll channel: " + self.ChannelId + " missing");

			int totalVotes = self.CountTotalVotes();

			StringBuilder description = new();

			description.AppendLine(self.Comment);
			description.AppendLine();

			bool isClosed = self.Closed();

			if (!isClosed)
			{
				description.Append("__Poll closes in ");
				description.Append(TimeUtils.GetDurationString(self.ClosesInstant, 1));
				description.AppendLine("__");
				description.AppendLine();
			}

			for (int i = 0; i < self.Options.Count; i++)
			{
				FC.Poll.Option op = self.Options[i];

				description.Append(PollService.ListEmotes[i]);
				description.Append(" - _");
				description.Append(op.Votes.Count);
				description.Append(op.Votes.Count == 1 ? " vote_ - **" : " votes_ - **");
				description.Append(op.Text);
				description.AppendLine("**");

				if (totalVotes < 20 && !self.Anon)
				{
					foreach (ulong userId in op.Votes)
					{
						string name = "Unknown";
						SocketGuildUser? user = channel.GetUser(userId);
						if (user != null)
							name = user.GetName();

						description.Append(Utils.Characters.Tab);
						description.Append(Utils.Characters.Tab);
						description.Append(Utils.Characters.Tab);
						description.Append(" - ");
						description.AppendLine(name);
					}

					if (op.Votes.Count > 0)
					{
						description.AppendLine();
					}
				}
			}

			StringBuilder title = new();
			SocketGuildUser? author = channel.GetUser(self.Author);

			/*if (author != null && CommandsService.GetPermissions(author) == Permissions.Administrators)
			{
				title.Append("FCChan");
			}*/

			title.Append(author == null ? "Someone" : author.GetName());

			if (isClosed)
			{
				title.Append(" asked:");
			}
			else
			{
				title.Append(" asks:");
			}

			EmbedBuilder builder = new EmbedBuilder
			{
				Title = title.ToString(),
				Footer = new EmbedFooterBuilder
				{
					Text = isClosed ? "Poll closed. Thanks for voting!" : "Vote for an option by selecting a reaction below",
				},
				Description = description.ToString(),
			};

			return Task.FromResult(builder.Build());
		}

		public static async Task UpdateMessage(this FC.Poll self)
		{
			Log.Write("Updating poll: \"" + self.Comment + "\" (" + self.Id + ")", "Bot");

			if (Program.DiscordClient.GetChannel(self.ChannelId) is not SocketTextChannel channel)
				throw new Exception("Poll channel: " + self.ChannelId + " missing");

			Embed embed = await self.ToEmbed();
			RestUserMessage? pollMessage = null;

			if (self.MessageId != 0)
				pollMessage = await channel.GetMessageAsync(self.MessageId) as RestUserMessage;

			if (pollMessage == null)
			{
				pollMessage = await channel.SendMessageAsync(null, false, embed);
				self.MessageId = pollMessage.Id;

				for (int i = 0; i < self.Options.Count; i++)
				{
					await pollMessage.AddReactionAsync(new Emoji(PollService.ListEmotes[i]));
				}
			}
			else
			{
				await pollMessage.ModifyAsync((x) =>
				{
					x.Embed = embed;
				});
			}
		}

		public static void Vote(this FC.Poll self, ulong userId, int optionIndex)
		{
			foreach (FC.Poll.Option op in self.Options)
			{
				op.Votes.Remove(userId);
			}

			self.Options[optionIndex].Votes.Add(userId);
		}
	}
}
