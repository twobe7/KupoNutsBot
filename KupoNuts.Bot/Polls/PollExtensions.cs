// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Polls
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Commands;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Utils;
	using NodaTime;

	public static class PollExtensions
	{
		public static async Task<bool> IsValid(this Poll self)
		{
			SocketTextChannel? channel = Program.DiscordClient.GetChannel(self.ChannelId) as SocketTextChannel;
			if (channel is null)
				return false;

			if (self.MessageId != 0)
			{
				RestUserMessage? pollMessage = await channel.GetMessageAsync(self.MessageId) as RestUserMessage;
				if (pollMessage == null)
				{
					return false;
				}
			}

			return true;
		}

		public static async Task Close(this Poll self)
		{
			Log.Write("Closing poll: \"" + self.Comment + "\" (" + self.Id + ")", "Bot");

			SocketTextChannel? channel = Program.DiscordClient.GetChannel(self.ChannelId) as SocketTextChannel;
			if (channel is null)
				return;

			RestUserMessage? pollMessage = await channel.GetMessageAsync(self.MessageId) as RestUserMessage;
			if (pollMessage != null)
				await pollMessage.RemoveAllReactionsAsync();

			self.ClosesInstant = TimeUtils.Now;
		}

		public static Task<Embed> ToEmbed(this Poll self)
		{
			SocketTextChannel? channel = Program.DiscordClient.GetChannel(self.ChannelId) as SocketTextChannel;

			if (channel == null)
				throw new Exception("Poll channel: " + self.ChannelId + " missing");

			int totalVotes = self.CountTotalVotes();

			StringBuilder description = new StringBuilder();

			description.AppendLine(self.Comment);
			description.AppendLine();

			if (!self.Closed())
			{
				description.Append("__Poll closes in ");
				description.Append(TimeUtils.GetDurationString(self.ClosesInstant));
				description.AppendLine("__");
				description.AppendLine();
			}

			for (int i = 0; i < self.Options.Count; i++)
			{
				Poll.Option op = self.Options[i];

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

			StringBuilder title = new StringBuilder();
			SocketGuildUser? author = channel.GetUser(self.Author);

			if (author != null && CommandsService.GetPermissions(author) == Permissions.Administrators)
			{
				title.Append("Kupo Nuts");
			}
			else
			{
				title.Append(author == null ? "Someone" : author.GetName());
			}

			if (self.Closed())
			{
				title.Append(" asked:");
			}
			else
			{
				title.Append(" asks:");
			}

			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = title.ToString();
			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = self.Closed() ? "Poll closed. thanks for voting!" : "Vote for an option by selecting a reaction below";
			builder.Description = description.ToString();

			return Task.FromResult(builder.Build());
		}

		public static async Task UpdateMessage(this Poll self)
		{
			Log.Write("Updating poll: \"" + self.Comment + "\" (" + self.Id + ")", "Bot");
			SocketTextChannel? channel = Program.DiscordClient.GetChannel(self.ChannelId) as SocketTextChannel;

			if (channel == null)
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

		public static void Vote(this Poll self, ulong userId, int optionIndex)
		{
			foreach (Poll.Option op in self.Options)
			{
				op.Votes.Remove(userId);
			}

			self.Options[optionIndex].Votes.Add(userId);
		}
	}
}
