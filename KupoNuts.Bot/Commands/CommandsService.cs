// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Services;

	public class CommandsService : ServiceBase
	{
		private const string CommandCharacter = "\\";

		private static Dictionary<string, Command> commandHandlers = new Dictionary<string, Command>();

		public static void BindCommand(string command, Func<string[], SocketMessage, Task> handler, Permissions permissions, string help)
		{
			command = command.ToLower();

			if (commandHandlers.ContainsKey(command))
				throw new Exception("Attempt to bind multiple commands with the same name");

			commandHandlers.Add(command, new Command(handler, permissions, help));
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

			BindCommand("help", this.Help, Permissions.Everyone, "Shows a list of available commands.");

			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.MessageReceived -= this.OnMessageReceived;

			ClearCommand("help");

			return Task.CompletedTask;
		}

		private static Permissions GetPermissions(SocketUser user)
		{
			if (user is SocketGuildUser guildUser)
			{
				foreach (SocketRole role in guildUser.Roles)
				{
					if (role.Permissions.Administrator)
					{
						return Permissions.Administrators;
					}
				}
			}

			return Permissions.Everyone;
		}

		private async Task Help(string[] args, SocketMessage message)
		{
			StringBuilder builder = new StringBuilder();

			Permissions permissions = GetPermissions(message.Author);

			List<string> commandStrings = new List<string>(commandHandlers.Keys);
			commandStrings.Sort();

			foreach (string commandString in commandStrings)
			{
				Command command = commandHandlers[commandString];

				// Don't show commands users cannot access
				if (command.Permission > permissions)
					continue;

				builder.Append("**\\");
				builder.Append(commandString);
				builder.Append("** - *");
				builder.Append(command.Permission);
				builder.Append("* - ");
				builder.AppendLine(command.Help);
			}

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Description = builder.ToString();
			await message.Channel.SendMessageAsync(null, false, embedBuilder.Build());
		}

		private bool HasPermission(SocketUser user, string command)
		{
			if (commandHandlers.ContainsKey(command))
			{
				Permissions requiredPermissions = commandHandlers[command].Permission;
				return requiredPermissions <= GetPermissions(user);
			}

			// not a command, so they _do_ have permission to try.
			return true;
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

			bool persmission = this.HasPermission(message.Author, command);

			Log.Write("Recieved command: " + command + " with " + message.Content + " Permission: " + persmission);

			if (!persmission)
			{
				await message.Channel.SendMessageAsync("I'm sorry, you don't have permission to do that~");
				return;
			}

			if (commandHandlers.ContainsKey(command))
			{
				try
				{
					await commandHandlers[command].Method.Invoke(args, message);
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

		private class Command
		{
			public readonly Func<string[], SocketMessage, Task> Method;
			public readonly Permissions Permission;
			public readonly string Help;

			public Command(Func<string[], SocketMessage, Task> method, Permissions permissions, string help)
			{
				this.Method = method;
				this.Permission = permissions;
				this.Help = help;
			}
		}
	}
}
