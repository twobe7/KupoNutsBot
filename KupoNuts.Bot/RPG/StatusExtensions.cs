// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG
{
	using System.Collections.Generic;
	using System.Text;
	using Discord;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.RPG.Items;
	using KupoNuts.RPG;

	public static class StatusExtensions
	{
		public static Embed ToEmbed(this Status status, IGuildUser user)
		{
			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = user.GetName();

			StringBuilder descBuilder = new StringBuilder();
			descBuilder.Append("Level: ");
			descBuilder.Append(status.Level.ToString());
			descBuilder.Append(Utils.Characters.DoubleTab);

			descBuilder.Append(RPGService.NutEmoteStr);
			descBuilder.Append(" ");
			descBuilder.AppendLine(status.Nuts.ToString());
			descBuilder.AppendLine();

			if (status.GetNumItems() > 0)
			{
				descBuilder.AppendLine("__Inventory__");
				foreach (Status.ItemStack stack in status.Inventory)
				{
					if (stack.Count <= 0)
						continue;

					ItemBase item = RPGService.GetItem(stack.ItemId);

					descBuilder.Append(item.Name);

					if (stack.Count > 1)
					{
						descBuilder.Append(" x");
						descBuilder.Append(stack.Count);
					}

					descBuilder.AppendLine();
				}

				descBuilder.AppendLine();
				descBuilder.Append("Use an item with the command: ");
				descBuilder.Append(CommandsService.CommandPrefixes.GetRandom());
				descBuilder.AppendLine("UseItem \"Item Name\" @target");
			}

			builder.Description = descBuilder.ToString();

			return builder.Build();
		}

		public static int GetNumItems(this Status status)
		{
			int itemCount = 0;
			foreach (Status.ItemStack stack in status.Inventory)
			{
				if (stack.Count <= 0)
					continue;

				itemCount++;
			}

			return itemCount;
		}

		public static Status.ItemStack? GetItem(this Status status, string id)
		{
			foreach (Status.ItemStack stack in status.Inventory)
			{
				if (stack.Count <= 0)
					continue;

				if (stack.ItemId == id)
				{
					return stack;
				}
			}

			return null;
		}
	}
}
