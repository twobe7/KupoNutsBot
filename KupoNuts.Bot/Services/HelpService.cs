// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;

	public class HelpService : ServiceBase
	{
		public static string GetTypeName(Type type)
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

			if (type == typeof(SocketTextChannel))
				return "#channel";

			if (type == typeof(IEmote))
				return ":emote:";

			if (type == typeof(IUser))
				return "@user";

			if (type == typeof(IGuildUser))
				return "@user";

			return type.Name;
		}

		public static string GetParam(ParameterInfo param)
		{
			string? name = param.Name;
			Type type = param.ParameterType;

			if (name == null)
				return "unknown";

			name = Regex.Replace(name, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
			name = "[" + name + "]";

			if (type == typeof(string))
				name = "\"" + name + "\"";

			if (type == typeof(IEmote))
				name = ":" + name + ":";

			if (type == typeof(IUser) || type == typeof(IGuildUser))
				name = "@" + name;

			return name;
		}

		public static Task<Embed> GetHelp(CommandMessage message, string? command = null)
		{
			StringBuilder builder = new StringBuilder();
			Permissions permissions = CommandsService.GetPermissions(message.Author);

			if (command == null)
				command = message.Command;

			builder.AppendLine(GetHelp(command, message.CommandPrefix, permissions));

			EmbedBuilder embed = new EmbedBuilder();
			embed.Description = builder.ToString();

			if (string.IsNullOrEmpty(embed.Description))
				throw new UserException("I'm sorry, you dont have permission to use that command.");

			return Task.FromResult(embed.Build());
		}

		public static Task<Embed> GetHelp(CommandMessage message, Permissions permissions)
		{
			StringBuilder builder = new StringBuilder();

			List<string> commandStrings = new List<string>(CommandsService.GetCommands());
			commandStrings.Sort();

			foreach (string commandString in commandStrings)
			{
				if (commandString == "help")
					continue;

				int count = 0;
				List<Command> commands = CommandsService.GetCommands(commandString);
				foreach (Command command in commands)
				{
					if (command.Permission > permissions)
						continue;

					count++;
				}

				if (count <= 0)
					continue;

				builder.Append("__");
				builder.Append(commandString);
				builder.Append("__ - *x");
				builder.Append(count);
				builder.Append("* - ");
				builder.Append(commands[0].Help);
				builder.AppendLine();
			}

			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("To get more information on a command, look it up directly, like `" + message.CommandPrefix + "help \"time\"` or `" + message.CommandPrefix + "goodbot ?`");

			EmbedBuilder embed = new EmbedBuilder();
			embed.Description = builder.ToString();

			return Task.FromResult(embed.Build());
		}

		[Command("Help", Permissions.Everyone, "really?")]
		public async Task<Embed> Help(CommandMessage message)
		{
			Permissions permissions = CommandsService.GetPermissions(message.Author);
			return await GetHelp(message, permissions);
		}

		[Command("Help", Permissions.Everyone, "really really?")]
		public async Task<Embed> Help(CommandMessage message, string command)
		{
			return await GetHelp(message, command);
		}

		private static string? GetHelp(string commandStr, string prefix, Permissions permissions)
		{
			StringBuilder builder = new StringBuilder();
			List<Command> commands = CommandsService.GetCommands(commandStr);

			int count = 0;
			foreach (Command command in commands)
			{
				// Don't show commands users cannot access
				if (command.Permission > permissions)
					continue;

				count++;
			}

			if (count <= 0)
				return null;

			builder.Append("__");
			////builder.Append(CommandsService.CommandPrefix);
			builder.Append(commandStr);
			builder.AppendLine("__");

			foreach (Command command in commands)
			{
				// Don't show commands users cannot access
				if (command.Permission > permissions)
					continue;

				builder.Append(Utils.Characters.Tab);
				builder.Append(command.Permission);
				builder.Append(" - *");
				builder.Append(command.Help);
				builder.AppendLine("*");

				List<ParameterInfo> parameters = command.GetNeededParams();

				builder.Append("**");
				builder.Append(Utils.Characters.Tab);
				builder.Append(prefix);
				builder.Append(commandStr);
				builder.Append(" ");

				for (int i = 0; i < parameters.Count; i++)
				{
					if (i != 0)
						builder.Append(", ");

					builder.Append(GetParam(parameters[i]));
				}

				builder.Append("**");
				builder.AppendLine();
				builder.AppendLine();
			}

			return builder.ToString();
		}
	}
}
