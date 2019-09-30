// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Services;

	public class CommandsService : ServiceBase
	{
		public static readonly List<string> CommandPrefixes = new List<string>()
		{
			">>",
			"?",
		};

		private static Dictionary<string, List<Command>> commandHandlers = new Dictionary<string, List<Command>>();

		public static void BindCommands(object obj)
		{
			Dictionary<MethodInfo, List<CommandAttribute>> commands = CommandAttribute.GetCommands(obj.GetType());

			foreach ((MethodInfo method, List<CommandAttribute> attributes) in commands)
			{
				foreach (CommandAttribute attribute in attributes)
				{
					if (!commandHandlers.ContainsKey(attribute.Command))
						commandHandlers.Add(attribute.Command, new List<Command>());

					Command cmd = new Command(method, obj, attribute.Permissions, attribute.Help);
					commandHandlers[attribute.Command].Add(cmd);
					Log.Write("Registered command: \"" + attribute.Command + "\"", "Bot");
				}
			}
		}

		public static IReadOnlyCollection<string> GetCommands()
		{
			return commandHandlers.Keys;
		}

		public static List<Command> GetCommands(string command)
		{
			if (!commandHandlers.ContainsKey(command.ToLower()))
				throw new UserException("I dont know that command.");

			return commandHandlers[command.ToLower()];
		}

		public static Permissions GetPermissions(IGuildUser user)
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

			foreach (string prefix in CommandPrefixes)
			{
				// Ignore messages that do not start with the command character
				if (!message.Content.StartsWith(prefix))
					continue;

				string command = message.Content.Substring(prefix.Length);
				command = command.TrimStart(' ', '	');

				// command must contain an actual command (dont process "?" as a command)
				if (command.Length <= 0)
					continue;

				// the first letter of the input must be a letter, or a space (dont process "?????" or ">>>" as a command)
				char first = command[0];
				if (!char.IsLetter(first))
					return;

				await this.ProcessCommandInput(message, prefix, command);
				return;
			}
		}

		private async Task ProcessCommandInput(SocketMessage message, string prefixUsed, string command)
		{
			string[] parts = Regex.Split(command, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
			////string[] parts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);

			command = parts[0];
			List<string> args = new List<string>();

			if (parts.Length > 1)
			{
				for (int i = 1; i < parts.Length; i++)
				{
					string arg = parts[i];
					////arg = arg.Replace("\"", string.Empty);

					if (string.IsNullOrEmpty(arg))
						continue;

					arg = arg.Replace("\\n", "\n");
					args.Add(arg);
				}
			}

			command = command.ToLower();
			CommandMessage cmdMessage = new CommandMessage(prefixUsed, command, message);

			if (args.Count == 1 && args[0] == "?")
			{
				await HelpService.GetHelp(cmdMessage);
				return;
			}

			Log.Write("Recieved command: " + command + " with " + message.Content + " From user: " + message.Author.Id, "Bot");
			_ = Task.Run(async () => await this.RunCommand(command, args.ToArray(), cmdMessage));
		}

		private async Task RunCommand(string commandStr, string[] args, CommandMessage message)
		{
			if (commandHandlers.ContainsKey(commandStr))
			{
				SocketTextChannel? textChannel = message.Channel as SocketTextChannel;

				if (textChannel == null)
					return;

				using (textChannel.EnterTypingState())
				{
					Exception? lastException = null;
					foreach (Command command in commandHandlers[commandStr])
					{
						try
						{
							lastException = null;
							await command.Invoke(args, message);
							break;
						}
						catch (ParameterException ex)
						{
							lastException = ex;
						}
						catch (Exception ex)
						{
							lastException = ex;
							break;
						}
					}

					if (lastException != null)
					{
						if (lastException is UserException userEx)
						{
							await message.Channel.SendMessageAsync(userEx.Message);
						}
						else if (lastException is ParameterException paramEx)
						{
							await message.Channel.SendMessageAsync(paramEx.Message);
						}
						else if (lastException is NotImplementedException)
						{
							await message.Channel.SendMessageAsync("I'm sorry, seems like I dont quite know how to do that yet.");
						}
						else
						{
							Log.Write(lastException);
							await message.Channel.SendMessageAsync("I'm sorry, something went wrong while handling that.");
						}
					}
				}
			}
			else
			{
				await message.Channel.SendMessageAsync("I'm sorry, I didn't understand that command.");
			}
		}
	}
}
