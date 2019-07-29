// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord.WebSocket;

	public class CommandsService : ServiceBase
	{
		private const string CommandCharacter = "\\";

		private static Dictionary<string, Func<string[], SocketMessage, Task>> commandHandlers = new Dictionary<string, Func<string[], SocketMessage, Task>>();

		public static void BindCommand(string command, Func<string[], SocketMessage, Task> handler)
		{
			command = command.ToLower();

			if (commandHandlers.ContainsKey(command))
				throw new Exception("Attempt to bind multiple commands with the same name");

			Log.Write("Bind command: " + command);
			commandHandlers.Add(command, handler);
		}

		public static void ClearCommand(string command)
		{
			command = command.ToLower();

			if (!commandHandlers.ContainsKey(command))
				return;

			commandHandlers.Remove(command);
		}

		public override Task Initialize()
		{
			Program.DiscordClient.MessageReceived += this.OnMessageReceived;
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.MessageReceived -= this.OnMessageReceived;
			return Task.CompletedTask;
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			// Ignore messages that did not come from users
			if (!(message is SocketUserMessage))
				return;

			// Ignore our own messages
			if (message.Author.Id == Program.DiscordClient.CurrentUser.Id)
				return;

			// Ignore messages that do not start with the command character
			if (!message.Content.StartsWith(CommandCharacter))
				return;

			string command = message.Content.Substring(1);
			string[] parts = command.Split(" ");

			command = parts[0];
			string[] args = new string[0];

			if (parts.Length > 1)
			{
				args = new string[parts.Length - 1];
				for (int i = 0; i < args.Length; i++)
				{
					args[i] = parts[i + 1];
				}
			}

			command = command.ToLower();

			Log.Write("Recieved command: " + command + " with " + message.Content);

			if (commandHandlers.ContainsKey(command))
			{
				try
				{
					await commandHandlers[command].Invoke(args, message);
				}
				catch (NotImplementedException)
				{
					await message.Channel.SendMessageAsync("I'm sorry, seems like I dont quite know how to do that yet.");
				}
				catch (Exception ex)
				{
					Log.Write(ex);
					await message.Channel.SendMessageAsync("I'm sorry, something went wrong while handling that.");
				}
			}
			else
			{
				await message.Channel.SendMessageAsync("I'm sorry, I didn't understand that command.");
			}

			await Task.Delay(0);
		}
	}
}
