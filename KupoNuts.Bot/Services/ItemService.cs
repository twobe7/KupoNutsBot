// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using KupoNuts.Bot.Commands;
	using XIVAPI;

	public class ItemService : ServiceBase
	{
		[Command("ISearch", Permissions.Everyone, "Gets information on an item")]
		[Command("ItemSearch", Permissions.Everyone, "Gets information on an item")]
		public async Task<Embed> GetItem(string search)
		{
			List<SearchAPI.Result> results = await SearchAPI.Search(search, ContentAPI.ContentType.Item);

			if (results.Count <= 0)
				throw new UserException("I couldn't find any items that match that search.");

			if (results.Count > 1)
			{
				EmbedBuilder embed = new EmbedBuilder();

				StringBuilder description = new StringBuilder();
				for (int i = 0; i < Math.Min(results.Count, 10); i++)
				{
					description.AppendLine(results[i].ID + " - " + results[i].Name);
				}

				embed.Title = results.Count + " results found";
				embed.Description = description.ToString();
				return embed.Build();
			}

			ulong? id = results[0].ID;

			if (id == null)
				throw new Exception("No Id in item");

			return await this.GetItem((ulong)id);
		}

		[Command("ISearch", Permissions.Everyone, "Gets information on an item")]
		[Command("ItemSearch", Permissions.Everyone, "Gets information on an item")]
		public async Task<Embed> GetItem(ulong itemID)
		{
			ContentAPI.Content content = await ContentAPI.Get(ContentAPI.ContentType.Item, itemID);

			StringBuilder desc = new StringBuilder();

			if (content.IsUnique == true || content.IsUntradable == true)
			{
				desc.Append("*");

				if (content.IsUnique == true)
					desc.Append("Unique");

				if (content.IsUnique == true && content.IsUntradable == true)
					desc.Append(" ");

				if (content.IsUntradable == true)
					desc.Append("Untradable");

				desc.AppendLine("*");
				desc.AppendLine();
			}

			if (content.ItemUICategory != null)
			{
				desc.Append("Lv. ");
				desc.Append(content.LevelEquip);
				desc.Append(" ");
				desc.AppendLine(content.ItemUICategory?.Name);
			}
			else
			{
				desc.Append("Lv. ");
				desc.Append(content.LevelEquip);
			}

			if (content.ClassJobCategory != null)
				desc.AppendLine(content.ClassJobCategory.Name);

			desc.Append("iLv. ");
			desc.AppendLine(content.LevelItem?.ToString());

			desc.AppendLine();
			bool indent = false;
			if (content.DamageMag != 0)
			{
				desc.Append("    __Magic Damage:__ ");
				desc.Append(content.DamageMag.ToString());
				indent = true;
			}

			if (content.DamagePhys != 0)
			{
				if (indent)
					desc.Append(Utils.Characters.Tab);

				desc.Append("    __Physical Damage:__ ");
				desc.Append(content.DamagePhys.ToString());
				indent = true;
			}

			if (content.DefenseMag != 0)
			{
				if (indent)
					desc.Append(Utils.Characters.Tab);

				desc.Append("    __Magical Defense:__ ");
				desc.Append(content.DefenseMag.ToString());
				indent = true;
			}

			if (content.DefensePhys != 0)
			{
				if (indent)
					desc.Append(Utils.Characters.Tab);

				desc.Append("    __Physical Defense:__ ");
				desc.Append(content.DefensePhys.ToString());
			}

			desc.AppendLine();

			if (!string.IsNullOrEmpty(content.Description))
				desc.AppendLine(content.Description);

			if (content.BaseParam0 != null)
			{
				desc.AppendLine();
				desc.Append(content.BaseParam0.Name);
				desc.Append(": ");
				desc.Append(content.BaseParamValue0);
			}

			if (content.BaseParam1 != null)
			{
				desc.Append(Utils.Characters.Tab);
				desc.Append(content.BaseParam1.Name);
				desc.Append(": ");
				desc.Append(content.BaseParamValue1);
			}

			if (content.BaseParam2 != null)
			{
				desc.Append(Utils.Characters.Tab);
				desc.Append(content.BaseParam2.Name);
				desc.Append(": ");
				desc.Append(content.BaseParamValue2);
			}

			if (content.BaseParam3 != null)
			{
				desc.Append(Utils.Characters.Tab);
				desc.Append(content.BaseParam3.Name);
				desc.Append(": ");
				desc.Append(content.BaseParamValue3);
			}

			if (content.BaseParam4 != null)
			{
				desc.Append(Utils.Characters.Tab);
				desc.Append(content.BaseParam4.Name);
				desc.Append(": ");
				desc.Append(content.BaseParamValue4);
			}

			if (content.BaseParam5 != null)
			{
				desc.Append(Utils.Characters.Tab);
				desc.Append(content.BaseParam5.Name);
				desc.Append(": ");
				desc.Append(content.BaseParamValue5);
			}

			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = content.Name;
			builder.Description = desc.ToString();
			builder.ThumbnailUrl = content.GetIcon();

			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = "ID: " + content.ID;

			return builder.Build();
		}
	}
}
