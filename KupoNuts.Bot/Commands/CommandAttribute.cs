// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;

	public class CommandAttribute : Attribute
	{
		public readonly string Command;
		public readonly Permissions Permissions;
		public readonly string Help;

		public CommandAttribute(string command, Permissions permissions, string help)
		{
			this.Command = command.ToLower();
			this.Permissions = permissions;
			this.Help = help;
		}

		public static Dictionary<MethodInfo, CommandAttribute> GetCommands(Type type)
		{
			Dictionary<MethodInfo, CommandAttribute> results = new Dictionary<MethodInfo, CommandAttribute>();

			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

			foreach (MethodInfo method in methods)
			{
				CommandAttribute? attribute = method.GetCustomAttribute<CommandAttribute>();

				if (attribute == null)
					continue;

				results.Add(method, attribute);
			}

			return results;
		}
	}
}
