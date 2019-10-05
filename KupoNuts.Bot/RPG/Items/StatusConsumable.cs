// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG.Items
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using KupoNuts.RPG;

	public class StatusConsumable : Consumable
	{
		public StatusConsumable(int id, string name, string desc, int cost, Func<IGuildUser, Status, Task<string>> func)
			: base(id, name, desc, cost, async (a, b) =>
			{
				Status status = await RPGService.GetStatus(b);
				string mesg = await func.Invoke(b, status);
				await RPGService.SaveStatus(status);
				return mesg;
			})
		{
		}
	}
}
