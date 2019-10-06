// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG.Items
{
	using System.Threading.Tasks;

	public class MacroItem : Consumable
	{
		public MacroItem(int id, string name, string desc, int cost, string macro)
			: base(id, name, desc, cost, (a, b) =>
			{
				string message = macro;

				message = message.Replace("<me>", a.Mention);
				message = message.Replace("<t>", b.Mention);

				return Task.FromResult(message);
			})
		{
		}
	}
}
