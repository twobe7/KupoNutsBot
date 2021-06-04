// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public class HelpCommand
	{
		public readonly string CommandName;
		public readonly CommandCategory CommandCategory;
		public readonly string Help;
		public readonly Permissions Permission;

		public HelpCommand(string name, CommandCategory category, string help, Permissions permission, string? shortcut = null)
		{
			this.CommandName = name;
			this.CommandCategory = category;
			this.Help = help;
			this.Permission = permission;
			this.CommandCount = 1;

			if (shortcut != null)
				this.CommandShortcuts.Add(shortcut);
		}

		public int CommandCount { get; set; }
		public List<string> CommandShortcuts { get; set; } = new List<string>();
	}
}
