// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Services;

	public class CommandsService : ServiceBase
	{
		private const string CommandPrefix = ">>";

		private static Dictionary<string, List<Command>> commandHandlers = new Dictionary<string, List<Command>>();

		public static void BindCommands(object obj)
		{
			Dictionary<MethodInfo, CommandAttribute> commands = CommandAttribute.GetCommands(obj.GetType());

			foreach ((MethodInfo method, CommandAttribute attribute) in commands)
			{
				if (!commandHandlers.ContainsKey(attribute.Command))
					commandHandlers.Add(attribute.Command, new List<Command>());

				Command cmd = new Command(method, obj, attribute.Permissions, attribute.Help);
				commandHandlers[attribute.Command].Add(cmd);
				Console.WriteLine("Registered command: \"" + attribute.Command + "\"");
			}
		}

		public static Permissions GetPermissions(SocketUser user)
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

		[Command("Help", Permissions.Everyone, "really?")]
		public async Task Help(SocketMessage message)
		{
			Permissions permissions = GetPermissions(message.Author);
			await GetHelp(message, permissions);
		}

		[Command("Help", Permissions.Everyone, "really really?")]
		public async Task Help(SocketMessage message, string command)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("```md");

			Permissions permissions = GetPermissions(message.Author);

			builder.AppendLine(GetHelp(command, permissions));

			builder.Append("```");
			await message.Channel.SendMessageAsync(builder.ToString());
		}

		[Command("Help", Permissions.Administrators, "really?")]
		public async Task Help(SocketMessage message, bool publicOnly)
		{
			await GetHelp(message, Permissions.Everyone);
		}

		private static async Task GetHelp(SocketMessage message, Permissions permissions)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("```md");

			List<string> commandStrings = new List<string>(commandHandlers.Keys);
			commandStrings.Sort();

			foreach (string commandString in commandStrings)
			{
				if (commandString == "help")
					continue;

				builder.AppendLine(GetHelp(commandString, permissions));
			}

			builder.Append("```");
			await message.Channel.SendMessageAsync(builder.ToString());
		}

		private static string GetHelp(string commandStr, Permissions permissions)
		{
			if (!commandHandlers.ContainsKey(commandStr.ToLower()))
				throw new UserException("I dont know that command.");

			StringBuilder builder = new StringBuilder();
			List<Command> commands = commandHandlers[commandStr.ToLower()];

			builder.Append("# ");
			builder.Append(CommandPrefix);
			builder.Append(commandStr);
			builder.AppendLine();

			foreach (Command command in commands)
			{
				// Don't show commands users cannot access
				if (command.Permission > permissions)
					continue;

				builder.Append(" < ");
				builder.Append(command.Permission);
				builder.Append(" > ");
				builder.AppendLine(command.Help);

				List<ParameterInfo> parameters = command.GetNeededParams();
				if (parameters.Count > 0)
				{
					builder.Append(" ");
					foreach (ParameterInfo param in parameters)
					{
						builder.Append("[");
						builder.Append(GetTypeName(param.ParameterType));
						builder.Append("](");
						builder.Append(GetParamName(param.Name));
						builder.Append("), ");
					}

					builder.AppendLine();
				}
			}

			return builder.ToString();
		}

		private static string GetTypeName(Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean: return "boolean";
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal: return "number";
				case TypeCode.DateTime: return "date and time";
				case TypeCode.String: return "string";
			}

			return type.Name;
		}

		private static string GetParamName(string? name)
		{
			if (name == null)
				return "unknown";

			return Regex.Replace(name, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
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
			if (!message.Content.StartsWith(CommandPrefix))
				return;

			string command = message.Content.Substring(CommandPrefix.Length);

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

					args.Add(arg);
				}
			}

			command = command.ToLower();

			if (args.Count == 1 && args[0] == "?")
			{
				await this.Help(message, command);
				return;
			}

			Log.Write("Recieved command: " + command + " with " + message.Content + " From user: " + message.Author.Id);
			_ = Task.Run(async () => await this.RunCommand(command, args.ToArray(), message));
		}

		private async Task RunCommand(string commandStr, string[] args, SocketMessage message)
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

		private class Command
		{
			public readonly MethodInfo Method;
			public readonly Permissions Permission;
			public readonly string Help;
			public readonly WeakReference<object> Owner;

			public Command(MethodInfo method, object owner, Permissions permissions, string help)
			{
				this.Method = method;
				this.Permission = permissions;
				this.Help = help;
				this.Owner = new WeakReference<object>(owner);
			}

			public List<ParameterInfo> GetNeededParams()
			{
				List<ParameterInfo> results = new List<ParameterInfo>();

				ParameterInfo[] allParamInfos = this.Method.GetParameters();
				for (int i = 0; i < allParamInfos.Length; i++)
				{
					ParameterInfo paramInfo = allParamInfos[i];

					if (paramInfo.ParameterType == typeof(SocketMessage))
						continue;

					if (paramInfo.ParameterType == typeof(string[]))
						continue;

					results.Add(paramInfo);
				}

				return results;
			}

			public async Task Invoke(string[] args, SocketMessage message)
			{
				if (this.Permission > CommandsService.GetPermissions(message.Author))
					throw new UserException("I'm sorry, you don't have permission to do that.");

				object? owner;
				if (!this.Owner.TryGetTarget(out owner) || owner == null)
					throw new Exception("Attempt to invoke command on null owner.");

				List<object> parameters = new List<object>();
				ParameterInfo[] allParamInfos = this.Method.GetParameters();

				int argCount = 0;
				int neededArgCount = allParamInfos.Length;
				for (int i = 0; i < allParamInfos.Length; i++)
				{
					ParameterInfo paramInfo = allParamInfos[i];

					if (paramInfo.ParameterType == typeof(SocketMessage))
					{
						parameters.Add(message);
						neededArgCount--;
						continue;
					}
					else if (paramInfo.ParameterType == typeof(string[]))
					{
						parameters.Add(args);
						neededArgCount--;
					}
					else
					{
						if (argCount >= args.Length)
						{
							parameters.Add("Empty");
							continue;
						}

						string arg = args[argCount];
						argCount++;

						object param;

						try
						{
							param = this.Convert(arg, paramInfo.ParameterType);
						}
						catch (Exception)
						{
							string hint = string.Empty;

							if (paramInfo.ParameterType == typeof(string))
								hint = "\n(Strings should have \"quotes\" around them, Kupo!)";

							throw new ParameterException("I didn't understand the parameter: " + arg + ".\nWas that was supposed to be a " + CommandsService.GetTypeName(paramInfo.ParameterType) + " for " + CommandsService.GetParamName(paramInfo.Name) + "?" + hint);
						}

						parameters.Add(param);
					}
				}

				if (parameters.Count != allParamInfos.Length || argCount != args.Length || neededArgCount != args.Length)
					throw new ParameterException("Incorrect number of parameters. I was expecting " + neededArgCount + ", but you sent me " + args.Length + "!");

				object? returnObject = this.Method.Invoke(owner, parameters.ToArray());

				if (returnObject is Task<string> tString)
				{
					string str = await tString;
					await message.Channel.SendMessageAsync(str);
				}
				else if (returnObject is Task task)
				{
					await task;
				}
			}

			private object Convert(string arg, Type type)
			{
				if (type == typeof(string))
				{
					if (!arg.Contains("\""))
						throw new Exception("strings must be wrapped in quotations");

					return arg.Replace("\"", string.Empty);
				}
				else if (type == typeof(double))
				{
					return double.Parse(arg);
				}
				else if (type == typeof(int))
				{
					return int.Parse(arg);
				}
				else if (type == typeof(uint))
				{
					return uint.Parse(arg);
				}
				else if (type == typeof(bool))
				{
					return bool.Parse(arg);
				}

				throw new Exception("Unsupported parameter type: \"" + type + "\"");
			}
		}

		private class ParameterException : Exception
		{
			public ParameterException(string message)
				: base(message)
			{
			}
		}
	}
}
