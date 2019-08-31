// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Utils;
	using KupoNuts.Utils;

	public class EchoService : ServiceBase
	{
		public static async Task<List<RestUserMessage>> Echo(SocketTextChannel from, SocketTextChannel to, ulong fromMessageID, int count)
		{
			List<RestUserMessage> results = new List<RestUserMessage>();

			List<IMessage> messages = new List<IMessage>(await from.GetMessagesAsync(fromMessageID, Discord.Direction.Before, count).FlattenAsync());

			for (int i = messages.Count - 1; i >= 0; i--)
			{
				IMessage prevMessage = messages[i];
				if (prevMessage.Embeds.Count > 0)
				{
					await from.SendMessageAsync("Sorry, I can't echo message embeds.");
					continue;
				}

				if (prevMessage.Attachments.Count == 1)
				{
					string attachmentURL = prevMessage.Attachments.Getfirst().Url;
					string filePath = "./Temp/" + prevMessage.Id + Path.GetExtension(attachmentURL);

					await FileDownloader.Download(attachmentURL, filePath);

					results.Add(await to.SendFileAsync(filePath, prevMessage.Content));
				}

				if (!string.IsNullOrEmpty(prevMessage.Content))
				{
					results.Add(await to.SendMessageAsync(prevMessage.Content, prevMessage.IsTTS));
				}
			}

			return results;
		}

		public override Task Initialize()
		{
			CommandsService.BindCommand("echo", this.HandleEcho, Permissions.Administrators, "Copies a range of messages to a new channel.");
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("echo");
			return Task.CompletedTask;
		}

		private async Task HandleEcho(string[] args, SocketMessage message)
		{
			if (args.Length != 2 || message.MentionedChannels.Count != 1)
			{
				await message.Channel.SendMessageAsync("You need to tell me how many messages and what channel to copy to! try \"\\echo 1 #channel-name\" to copy the previous 1 message to a new channel");
				return;
			}

			SocketGuildChannel toChannel = message.MentionedChannels.Getfirst();
			if (toChannel is SocketTextChannel toTextChannel)
			{
				int count = 1;
				int.TryParse(args[0], out count);

				await Echo((SocketTextChannel)message.Channel, toTextChannel, message.Id, count);
			}
			else
			{
				await message.Channel.SendMessageAsync("I can only echo messages into text channels!");
			}
		}
	}
}
