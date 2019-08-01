// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Utils;
	using KupoNuts.Utils;

	public class EchoService : ServiceBase
	{
		public override Task Initialize()
		{
			CommandsService.BindCommand("echo", this.Echo, Permissions.Administrators, "Copies a range of messages to a new channel.");
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("echo");
			return Task.CompletedTask;
		}

		private async Task Echo(string[] args, SocketMessage message)
		{
			if (args.Length != 2 || message.MentionedChannels.Count != 1)
			{
				await message.Channel.SendMessageAsync("You need to tell me how many messages and what channel to copy to! try \"\\echo 1 #channel-name\" to copy the previous 1 message to a new channel");
				return;
			}

			SocketGuildChannel channel = message.MentionedChannels.Getfirst();

			if (channel is SocketTextChannel textChannel)
			{
				IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync(message.Id, Discord.Direction.Before, 1).FlattenAsync();
				foreach (IMessage prevMessage in messages)
				{
					if (prevMessage.Embeds.Count > 0)
					{
						await message.Channel.SendMessageAsync("Sorry, I can't echo message embeds.");
						continue;
					}

					if (prevMessage.Attachments.Count == 1)
					{
						string attachmentURL = prevMessage.Attachments.Getfirst().Url;
						string filePath = "./Temp/" + prevMessage.Id + Path.GetExtension(attachmentURL);

						await FileDownloader.Download(attachmentURL, filePath);

						await textChannel.SendFileAsync(filePath, prevMessage.Content);
					}

					if (!string.IsNullOrEmpty(prevMessage.Content))
					{
						await textChannel.SendMessageAsync(prevMessage.Content, prevMessage.IsTTS);
					}
				}
			}
			else
			{
				await message.Channel.SendMessageAsync("I can only echo messages into text channels!");
			}
		}
	}
}
