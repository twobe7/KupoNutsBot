﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class CommandAttribute : Attribute
	{
		public readonly string Command;
		public readonly string CommandLower;
		public readonly Permissions Permissions;
		public readonly string Help;
		public readonly CommandCategory CommandCategory;
		public readonly string? CommandParent;
		public readonly bool RequiresQuotes;
		public readonly bool ShowWait;

		public CommandAttribute(string command, Permissions permissions, string help, CommandCategory commandCategory = CommandCategory.Miscellaneous, string? commandParent = null, bool requiresQuotes = false, bool showWait = true)
		{
			this.Command = command;
			this.CommandLower = this.Command.ToLower();
			this.CommandCategory = permissions == Permissions.Administrators ? CommandCategory.Administration : commandCategory;
			this.CommandParent = commandParent;
			this.Permissions = permissions;
			this.Help = help;
			this.RequiresQuotes = requiresQuotes;
			this.ShowWait = showWait;
		}

		public static Dictionary<MethodInfo, List<CommandAttribute>> GetCommands(Type type)
		{
			Dictionary<MethodInfo, List<CommandAttribute>> results = new Dictionary<MethodInfo, List<CommandAttribute>>();

			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

			foreach (MethodInfo method in methods)
			{
				foreach (CommandAttribute attribute in method.GetCustomAttributes<CommandAttribute>().OrderBy(x => x.CommandLower))
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
