// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Commands
{
	using Discord;
	using Discord.WebSocket;

	public class CommandMessage
	{
		public readonly string Command;
		public readonly SocketMessage Message;
		public readonly bool ShowWait;

		public CommandMessage(string command, SocketMessage message)
		{
			this.Command = command;
			this.Message = message;
		}

		public ulong Id => this.Message.Id;

		public ISocketMessageChannel Channel => this.Message.Channel;

		public IGuildUser Author => this.Message.GetAuthor();

		public IGuild Guild => this.Message.GetGuild();

		public MessageReference MessageReference => new MessageReference(this.Message.Id);

		public async void DeleteMessage()
		{
			await this.Message.DeleteAsync();
		}
	}
}
