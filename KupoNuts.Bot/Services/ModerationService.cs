// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Utils;

	public class ModerationService : ServiceBase
	{
		[Command("Clear", Permissions.Administrators, "Clears a range of messages from the channel")]
		public async Task Clear(CommandMessage message, int count)
		{
			IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync(count + 1).FlattenAsync();

			foreach (IMessage messageToRemove in messages)
			{
				await messageToRemove.DeleteAsync();
				await Task.Delay(100);
			}
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
			UserService.User usr = await UserService.GetUser(message.Guild.Id, id);
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
			UserService.User usr = await UserService.GetUser(message.Guild.Id, id);
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

			UserService.User user = await UserService.GetUser(message.GetAuthor());

			UserService.User.Warning warning = new UserService.User.Warning();
			warning.Action = UserService.User.Warning.Actions.PostRemoved;
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
	}
}
