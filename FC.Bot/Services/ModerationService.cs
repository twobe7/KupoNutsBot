// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Utils;

	public class ModerationService : ServiceBase
	{
		public ModerationService(DiscordSocketClient discordClient)
		{
		}

		[Command("ClearAll", Permissions.Administrators, "Clears a range of messages from the channel")]
		public async Task ClearAll(CommandMessage message, int count)
		{
			IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync(count + 1).FlattenAsync();

			foreach (IMessage messageToRemove in messages)
			{
				await messageToRemove.DeleteAsync();
				await Task.Delay(100);
			}
		}

		[Command("Clear", Permissions.Administrators, "Clears a range of messages from the channel, excluding pinned")]
		public async Task ClearPinned(CommandMessage message, int count)
		{
			int pinnedMessages = 0;

			IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync(count + 1).FlattenAsync();

			foreach (IMessage messageToRemove in messages)
			{
				if (messageToRemove.IsPinned)
				{
					pinnedMessages++;
					continue;
				}

				try
				{
					await messageToRemove.DeleteAsync();
					await Task.Delay(100);
				}
				catch (Exception)
				{
					// Do nothing, just skip the 404 that happens here sometimes
				}
			}

			// Remove extra messages to make up initial count if any of original messages were pinned
			////if (pinnedMessages > 0)
			////{
			////	messages = await message.Channel.GetMessagesAsync(pinnedMessages * 2).FlattenAsync();
			////	foreach (IMessage messageToRemove in messages.Skip(pinnedMessages))
			////	{
			////		try
			////		{
			////			await messageToRemove.DeleteAsync();
			////			await Task.Delay(100);
			////		}
			////		catch (Exception)
			////		{
			////			// Do nothing, just skip the 404 that happens here sometimes
			////		}
			////	}
			////}
		}

		[Command("Ban", Permissions.Administrators, "Bans a user")]
		public async Task Ban(CommandMessage message, IGuildUser user)
		{
			await message.Guild.AddBanAsync(user);
		}

		[Command("Unban", Permissions.Administrators, "Unbans a user")]
		public async Task Unban(CommandMessage message, IGuildUser user)
		{
			await message.Guild.AddBanAsync(user);
		}

		[Command("Unban", Permissions.Administrators, "Unbans a user")]
		public async Task Unban(CommandMessage message, ulong id)
		{
			User usr = await UserService.GetUser(message.Guild.Id, id);
			usr.Banned = false;
			await UserService.SaveUser(usr);

			IUser user = await message.Guild.GetUserAsync(id);
			if (user != null)
			{
				await message.Guild.RemoveBanAsync(user);
			}
		}

		[Command("Ban", Permissions.Administrators, "Bans a user")]
		public async Task<string> Ban(CommandMessage message, ulong id)
		{
			User usr = await UserService.GetUser(message.Guild.Id, id);
			usr.Banned = true;
			await UserService.SaveUser(usr);

			IUser user = await message.Guild.GetUserAsync(id);
			if (user == null)
				return "That user is not a member of the server. They will be banned if they rejoin.";

			await message.Guild.AddBanAsync(user);
			return user.Username + " has been banned.";
		}

		[Command("Remove", Permissions.Administrators, "Removes a message, posts the reason, and logs a warning")]
		public async Task<string> Remove(SocketTextChannel channel, ulong messageId, string reason)
		{
			IMessage message = await channel.GetMessageAsync(messageId);
			await channel.DeleteMessageAsync(message);

			User user = await UserService.GetUser(message.GetAuthor());

			User.Warning warning = new User.Warning();
			warning.Action = User.Warning.Actions.PostRemoved;
			warning.ChannelId = channel.Id;
			warning.Comment = reason;
			user.Warnings.Add(warning);
			await UserService.SaveUser(user);

			StringBuilder builder = new StringBuilder();
			builder.Append(message.GetAuthor().GetName());
			builder.Append(", a message you posted in the ");
			builder.Append(channel.Guild.Name);
			builder.Append(" #");
			builder.Append(channel.Name);
			builder.Append(" channel has been removed for the following reason: ");
			builder.Append(reason);
			builder.AppendLine(".");

			builder.Append("This is your ");
			builder.Append(user.Warnings.Count);
			builder.Append(NumberUtils.GetOrdinal(user.Warnings.Count));
			builder.Append(" warning.");

			builder.AppendLine();
			builder.AppendLine("This action was taken manually by a human moderator.");
			builder.Append("Please contact a moderator on the ");
			builder.Append(channel.Guild.Name);
			builder.Append(" server if you would like to discus this action.");

			await message.GetAuthor().SendMessageAsync(builder.ToString());

			builder.Clear();
			builder.AppendLine("Message removed.");
			builder.Append(message.GetAuthor().GetName());
			builder.Append(" has now been warned ");
			builder.Append(user.Warnings.Count);
			builder.Append(" times");
			return builder.ToString();
		}

		[Command("Log", Permissions.Administrators, "Logs a series of messages from a channel to a file, and send the file")]
		public async Task Log(CommandMessage cmdMessage, SocketTextChannel channel, int messagecount)
		{
			IEnumerable<IMessage> messages = await channel.GetMessagesAsync(messagecount).FlattenAsync();

			string directory = PathUtils.Current + "/Logs/";
			string path = directory + channel.Id + ".txt";

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			using (StreamWriter outputFile = new StreamWriter(path))
			{
				foreach (IMessage message in messages)
				{
					outputFile.Write(message.GetAuthor().GetName());
					outputFile.Write(" [");
					outputFile.Write(message.Timestamp);
					outputFile.Write("] ");
					outputFile.Write(message.Content);

					foreach (IEmbed embed in message.Embeds)
					{
						outputFile.WriteLine();
						outputFile.Write("    [Embed] ");
						outputFile.WriteLine(embed.Title);
						outputFile.Write("    ");
						outputFile.WriteLine(embed.Description);

						foreach (EmbedField field in embed.Fields)
						{
							outputFile.Write("    ");
							outputFile.Write(field.Name);
							outputFile.Write(" - ");
							outputFile.WriteLine(field.Value);
						}
					}

					foreach (IAttachment attachment in message.Attachments)
					{
						outputFile.WriteLine();
						outputFile.Write("    [Attachment] ");
						outputFile.Write(attachment.Filename);
						outputFile.Write(" - ");
						outputFile.Write(attachment.Url);
					}

					outputFile.WriteLine();
				}
			}

			await cmdMessage.Channel.SendFileAsync(path);
		}
	}
}
