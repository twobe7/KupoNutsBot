// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Items
{
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using Discord;
	using XIVAPI;

	public static class ItemExtensions
	{
		public static EmbedBuilder ToEmbed(this Item self)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = self.Name;
			builder.ThumbnailUrl = Icons.GetIconURL(self.Icon);

			StringBuilder desc = new StringBuilder();

			desc.Append(self.GetLevelCategoryString());
			desc.Append(Utils.Characters.DoubleTab);
			desc.Append(self.GetIconInfoString());
			desc.AppendLine();
			desc.Append(self.GetLevelClassString());
			desc.AppendLine();

			if (!string.IsNullOrEmpty(self.Description))
			{
				desc.AppendLine(self.GetDescription());
				desc.AppendLine();
			}

			// Garland tools link
			desc.Append("[Garland Tools Database](");
			desc.Append("http://www.garlandtools.org/db/#item/");
			desc.Append(self.ID);
			desc.AppendLine(")");

			// gamerescape link
			desc.Append("[Gamer Escape](");
			desc.Append("https://ffxiv.gamerescape.com/wiki/Special:Search/");
			desc.Append(self.Name.Replace(" ", "%20"));
			desc.AppendLine(")");

			if (self.HasMainStats())
				builder.AddField("Stats", self.GetMainStatsString());

			if (self.HasSecondaryStats())
				builder.AddField("Bonuses", self.GetSecondaryStatsString());

			if (self.HasSpecialStats())
				builder.AddField(self.GetSpecialStatsName(), self.GetSpecialStatsString());

			if (self.HasMateria())
				builder.AddField("Materia", self.GetMateriaString());

			if (self.HasBonuses())
				builder.AddField("Bonuses", self.GetBonuses());

			StringBuilder footerText = new StringBuilder();
			footerText.Append("ID: ");
			footerText.Append(self.ID.ToString());
			footerText.Append(" - XIVAPI.com - Universalis.app");

			builder.Description = desc.ToString();
			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = footerText.ToString();
			builder.Color = Color.Teal;

			return builder;
		}

		public static string GetDescription(this Item self)
		{
			if (self.Description == null)
				return string.Empty;

			string desc = self.Description;
			desc = Regex.Replace(self.Description, "<.*?>", string.Empty);

			while (desc.Contains("\n\n\n"))
				desc = desc.Replace("\n\n\n", "\n\n");

			return desc;
		}

		public static string GetIconInfoString(this Item self)
		{
			StringBuilder builder = new StringBuilder();

			if (self.AetherialReduce == true || self.MaterializeType != 0)
				builder.Append(ItemService.ConvertToMateriaEmote);

			if (self.IsDyeable == true)
				builder.Append(ItemService.DyeEmote);

			if (self.Salvage != null)
				builder.Append(ItemService.SalvageEmote);

			if (self.IsGlamourous == true)
				builder.Append(ItemService.GlamourDresserEmote);

			// such a hack. D=
			if (self.Json != null && self.Json.Contains("{\"Cabinet\":{\"Item\":"))
				builder.Append(ItemService.ArmoireEmote);

			if (self.IsUntradable == true)
				builder.Append(ItemService.UntradableEmote);

			if (self.IsUnique == true)
				builder.Append(ItemService.UniqueEmote);

			if (self.HasMateria() && self.IsAdvancedMeldingPermitted != true)
				builder.Append(ItemService.AdvancedMeldingForbiddenEmote);

			if (self.IsCraftable())
				builder.Append(ItemService.CraftableEmote);

			return builder.ToString();
		}

		public static string GetLevelCategoryString(this Item self)
		{
			StringBuilder builder = new StringBuilder();
			if (self.ItemUICategory != null)
			{
				builder.Append("Lv. ");
				builder.Append(self.LevelEquip);
				builder.Append(" - ");
				builder.Append(self.ItemUICategory?.Name);
			}
			else
			{
				builder.Append("Lv. ");
				builder.Append(self.LevelEquip.ToString());
			}

			return builder.ToString();
		}

		public static string GetLevelClassString(this Item self)
		{
			if (self.LevelItem == 1)
				return string.Empty;

			StringBuilder builder = new StringBuilder();
			builder.Append("iLv. ");
			builder.Append(self.LevelItem.ToString());

			if (self.ClassJobCategory != null)
			{
				builder.Append(" - ");
				builder.Append(self.ClassJobCategory.Name);
			}

			builder.AppendLine();

			return builder.ToString();
		}

		public static bool HasMainStats(this Item self)
		{
			return self.DamageMag != 0 || self.DamagePhys != 0 || self.DefenseMag != 0 || self.DefensePhys != 0 || self.DelayMs != 0;
		}

		public static string GetMainStatsString(this Item self)
		{
			StringBuilder builder = new StringBuilder();

			bool indent = false;
			if (self.DamageMag != 0)
			{
				builder.Append("Magic Damage: ");
				builder.Append(self.DamageMag.ToString());
				indent = true;
			}

			if (self.DamagePhys != 0)
			{
				if (indent)
					builder.Append(Utils.Characters.DoubleTab);

				builder.Append("Physical Damage: ");
				builder.Append(self.DamagePhys.ToString());
				indent = true;
			}

			if (self.DefenseMag != 0)
			{
				if (indent)
					builder.Append(Utils.Characters.DoubleTab);

				builder.Append("Magical Defense: ");
				builder.Append(self.DefenseMag.ToString());
				indent = true;
			}

			if (self.DefensePhys != 0)
			{
				if (indent)
					builder.Append(Utils.Characters.DoubleTab);

				builder.Append("Physical Defense: ");
				builder.Append(self.DefensePhys.ToString());
				indent = true;
			}

			if (self.DelayMs != 0)
			{
				if (indent)
					builder.Append(Utils.Characters.DoubleTab);

				builder.Append("Delay: ");
				builder.Append(self.DelayMs / 1000.0);
				builder.Append("s");
				indent = true;
			}

			return builder.ToString();
		}

		public static bool HasSecondaryStats(this Item self)
		{
			return self.BaseParam0 != null || self.BaseParam1 != null || self.BaseParam2 != null || self.BaseParam3 != null || self.BaseParam4 != null || self.BaseParam5 != null;
		}

		public static string GetSecondaryStatsString(this Item self)
		{
			StringBuilder builder = new StringBuilder();
			if (self.BaseParam0 != null)
			{
				builder.AppendLine();
				builder.Append(self.BaseParam0.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValue0);
			}

			if (self.BaseParam1 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParam1.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValue1);
			}

			if (self.BaseParam2 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParam2.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValue2);
			}

			if (self.BaseParam3 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParam3.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValue3);
			}

			if (self.BaseParam4 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParam4.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValue4);
			}

			if (self.BaseParam5 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParam5.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValue5);
			}

			return builder.ToString();
		}

		public static bool HasSpecialStats(this Item self)
		{
			return self.BaseParamSpecial0 != null || self.BaseParamSpecial1 != null || self.BaseParamSpecial2 != null || self.BaseParamSpecial3 != null || self.BaseParamSpecial4 != null || self.BaseParamSpecial5 != null;
		}

		public static string GetSpecialStatsName(this Item self)
		{
			if (self.ItemSpecialBonus != null && !string.IsNullOrEmpty(self.ItemSpecialBonus.Name))
				return self.ItemSpecialBonus.Name;

			return "Special Bonus";
		}

		public static string GetSpecialStatsString(this Item self)
		{
			StringBuilder builder = new StringBuilder();
			if (self.BaseParamSpecial0 != null)
			{
				builder.Append(self.BaseParamSpecial0.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValueSpecial0);
			}

			if (self.BaseParamSpecial1 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParamSpecial1.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValueSpecial1);
			}

			if (self.BaseParamSpecial2 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParamSpecial2.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValueSpecial2);
			}

			if (self.BaseParamSpecial3 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParamSpecial3.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValueSpecial3);
			}

			if (self.BaseParamSpecial4 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParamSpecial4.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValueSpecial4);
			}

			if (self.BaseParamSpecial5 != null)
			{
				builder.Append(Utils.Characters.DoubleTab);
				builder.Append(self.BaseParamSpecial5.Name);
				builder.Append(": +");
				builder.Append(self.BaseParamValueSpecial5);
			}

			return builder.ToString();
		}

		public static bool HasMateria(this Item self)
		{
			return self.MateriaSlotCount > 0;
		}

		public static bool IsCraftable(this Item self)
		{
			return self.GameContentLinks != null && self.GameContentLinks.Recipe != null;
		}

		public static string GetMateriaString(this Item self)
		{
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < self.MateriaSlotCount; i++)
				builder.Append("◯ ");

			builder.AppendLine();

			return builder.ToString();
		}

		public static bool HasBonuses(this Item self)
		{
			if (self.Bonuses == null)
				return false;

			bool hasBonus = false;
			hasBonus |= self.Bonuses.Piety != null;
			hasBonus |= self.Bonuses.SpellSpeed != null;
			hasBonus |= self.Bonuses.Vitality != null;
			hasBonus |= self.Bonuses.Tenacity != null;
			hasBonus |= self.Bonuses.Determination != null;
			hasBonus |= self.Bonuses.DirectHit != null;
			hasBonus |= self.Bonuses.SkillSpeed != null;
			hasBonus |= self.Bonuses.CriticalHit != null;
			hasBonus |= self.Bonuses.CP != null;
			hasBonus |= self.Bonuses.Control != null;
			hasBonus |= self.Bonuses.Perception != null;
			hasBonus |= self.Bonuses.GP != null;
			hasBonus |= self.Bonuses.Gathering != null;
			hasBonus |= self.Bonuses.Craftsmanship != null;

			return hasBonus;
		}

		public static string GetBonuses(this Item self)
		{
			StringBuilder builder = new StringBuilder();

			self.Bonuses?.Piety?.GetBonus(ref builder, "Piety");
			self.Bonuses?.SpellSpeed?.GetBonus(ref builder, "Spell Speed");
			self.Bonuses?.Vitality?.GetBonus(ref builder, "Vitality");
			self.Bonuses?.Tenacity?.GetBonus(ref builder, "Tenacity");
			self.Bonuses?.Determination?.GetBonus(ref builder, "Determination");
			self.Bonuses?.DirectHit?.GetBonus(ref builder, "Direct Hit");
			self.Bonuses?.SkillSpeed?.GetBonus(ref builder, "Skill Speed");
			self.Bonuses?.CriticalHit?.GetBonus(ref builder, "Critical Hit");
			self.Bonuses?.CP?.GetBonus(ref builder, "CP");
			self.Bonuses?.Control?.GetBonus(ref builder, "Control");
			self.Bonuses?.Perception?.GetBonus(ref builder, "Perception");
			self.Bonuses?.GP?.GetBonus(ref builder, "GP");
			self.Bonuses?.Gathering?.GetBonus(ref builder, "Gathering");
			self.Bonuses?.Craftsmanship?.GetBonus(ref builder, "Craftsmanship");

			return builder.ToString();
		}

		private static void GetBonus(this Item.StatBonus bonus, ref StringBuilder builder, string name)
		{
			builder.Append(name);
			builder.Append(": ");

			builder.Append(ItemService.HighQualityEmote);
			builder.Append(" ");
			builder.Append(bonus.ValueHq);

			if (bonus.Relative)
				builder.Append("%");

			builder.Append(" (Max: ");
			builder.Append(bonus.MaxHq);
			builder.Append(") ");

			builder.Append(ItemService.NormalQualityEmote);
			builder.Append(" ");
			builder.Append(bonus.Value);

			if (bonus.Relative)
				builder.Append("%");

			builder.Append(" (Max: ");
			builder.Append(bonus.Max);
			builder.AppendLine(")");
		}
	}
}
