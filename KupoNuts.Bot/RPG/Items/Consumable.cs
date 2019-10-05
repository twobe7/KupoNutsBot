// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG.Items
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;

	public class Consumable : ItemBase
	{
		private Func<IGuildUser, IGuildUser, Task<string>> func;

		public Consumable(int id, string name, string desc, int cost, Func<IGuildUser, IGuildUser, Task<string>> func)
			: base(id, name, desc, cost)
		{
			this.func = func;
		}

		public async Task<string> Use(IGuildUser source, IGuildUser target)
		{
			return await this.func.Invoke(source, target);
		}
	}
}
