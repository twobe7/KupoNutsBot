// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Items
{
	using System.Text;
	using Discord;
	using XIVAPI;

	public static class ItemExtensions
	{
		public static bool IsUniqueOrUntradable(this ItemAPI.Item self)
		{
			return self.IsUnique == true || self.IsUntradable == true;
		}

		public static Embed ToEmbed(this ItemAPI.Item self)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = self.Name;
			builder.ThumbnailUrl = Icons.GetIconURL(self.Icon);

			StringBuilder desc = new StringBuilder();

			desc.Append(self.GetLevelCategoryString());
			desc.Append(self.GetLevelClassString());

			if (self.IsUniqueOrUntradable())
				desc.AppendLine(self.GetUniqueUntradableString());

			if (self.HasMainStats())
				builder.AddField("Stats", self.GetMainStatsString());

			if (!string.IsNullOrEmpty(self.Description))
				desc.AppendLine(self.Description);

			if (self.HasSecondaryStats())
				builder.AddField("Bonuses", self.GetSecondaryStatsString());

			if (self.HasSpecialStats())
				builder.AddField(self.GetSpecialStatsName(), self.GetSpecialStatsString());

			if (self.HasMateria())
				builder.AddField("Materia", self.GetMateriaString());

			builder.Description = desc.ToString();
			builder.Footer = new EmbedFooterBuilder();
			builder.Footer.Text = self.ID?.ToString();

			builder.Url = "http://www.garlandtools.org/db/#item/" + self.ID?.ToString();

			return builder.Build();
		}

		public static string GetUniqueUntradableString(this ItemAPI.Item self)
		{
			StringBuilder builder = new StringBuilder();
			if (self.IsUnique == true || self.IsUntradable == true)
			{
				builder.Append("*");

				if (self.IsUnique == true)
					builder.Append("Unique");

				if (self.IsUnique == true && self.IsUntradable == true)
					builder.Append(Utils.Characters.Tab);

				if (self.IsUntradable == true)
					builder.Append("Untradable");

				builder.Append("*");
			}

			return builder.ToString();
		}

		public static string GetLevelCategoryString(this ItemAPI.Item self)
		{
			StringBuilder builder = new StringBuilder();
			if (self.ItemUICategory != null)
			{
				builder.Append("Lv. ");
				builder.Append(self.LevelEquip);
				builder.Append(" ");
				builder.AppendLine(self.ItemUICategory?.Name);
			}
			else
			{
				builder.Append("Lv. ");
				builder.AppendLine(self.LevelEquip?.ToString());
			}

			return builder.ToString();
		}

		public static string GetLevelClassString(this ItemAPI.Item self)
		{
			StringBuilder builder = new StringBuilder();

			if (self.ClassJobCategory != null)
				builder.AppendLine(self.ClassJobCategory.Name);

			builder.Append("iLv. ");
			builder.AppendLine(self.LevelItem?.ToString());

			return builder.ToString();
		}

		public static bool HasMainStats(this ItemAPI.Item self)
		{
			return self.DamageMag != 0 || self.DamagePhys != 0 || self.DefenseMag != 0 || self.DefensePhys != 0 || self.DelayMs != 0;
		}

		public static string GetMainStatsString(this ItemAPI.Item self)
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

		public static bool HasSecondaryStats(this ItemAPI.Item self)
		{
			return self.BaseParam0 != null || self.BaseParam1 != null || self.BaseParam2 != null || self.BaseParam3 != null || self.BaseParam4 != null || self.BaseParam5 != null;
		}

		public static string GetSecondaryStatsString(this ItemAPI.Item self)
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

		public static bool HasSpecialStats(this ItemAPI.Item self)
		{
			return self.BaseParamSpecial0 != null || self.BaseParamSpecial1 != null || self.BaseParamSpecial2 != null || self.BaseParamSpecial3 != null || self.BaseParamSpecial4 != null || self.BaseParamSpecial5 != null;
		}

		public static string GetSpecialStatsName(this ItemAPI.Item self)
		{
			if (self.ItemSpecialBonus != null && !string.IsNullOrEmpty(self.ItemSpecialBonus.Name))
				return self.ItemSpecialBonus.Name;

			return "Special Bonus";
		}

		public static string GetSpecialStatsString(this ItemAPI.Item self)
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

		public static bool HasMateria(this ItemAPI.Item self)
		{
			return (self.MateriaSlotCount != null && self.MateriaSlotCount > 0) || self.IsAdvancedMeldingPermitted != null;
		}

		public static string GetMateriaString(this ItemAPI.Item self)
		{
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < self.MateriaSlotCount; i++)
				builder.Append("◯ ");

			builder.AppendLine();

			if (self.IsAdvancedMeldingPermitted == false)
				builder.AppendLine("Advanced Melding Forbidden");

			return builder.ToString();
		}
	}
}
