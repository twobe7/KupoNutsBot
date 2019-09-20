// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
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

		public static Dictionary<MethodInfo, List<CommandAttribute>> GetCommands(Type type)
		{
			Dictionary<MethodInfo, List<CommandAttribute>> results = new Dictionary<MethodInfo, List<CommandAttribute>>();

			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

			foreach (MethodInfo method in methods)
			{
				foreach (CommandAttribute attribute in method.GetCustomAttributes<CommandAttribute>())
				{
					if (!results.ContainsKey(method))
						results.Add(method, new List<CommandAttribute>());

					results[method].Add(attribute);
				}
			}

			return results;
		}
	}
}
