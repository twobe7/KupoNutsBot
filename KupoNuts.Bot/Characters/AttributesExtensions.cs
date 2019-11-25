// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using XIVAPI;

	public enum AttributeCategories
	{
		Attributes,
		Offensive,
		Defensive,
		Physical,
		Mental,
		Role,
		Other,

		Unknown,
	}

	public static class AttributesExtensions
	{
		public static AttributeCategories GetCategory(this AttributeValue value)
		{
			if (value.Attribute == null)
				return AttributeCategories.Attributes;

			switch (value.Attribute.ID)
			{
				case 1: return AttributeCategories.Attributes;
				case 2: return AttributeCategories.Attributes;
				case 3: return AttributeCategories.Attributes;
				case 4: return AttributeCategories.Attributes;
				case 5: return AttributeCategories.Attributes;

				case 27: return AttributeCategories.Offensive;
				case 44: return AttributeCategories.Offensive;
				case 22: return AttributeCategories.Offensive;

				case 21: return AttributeCategories.Defensive;
				case 24: return AttributeCategories.Defensive;

				case 20: return AttributeCategories.Physical;
				case 45: return AttributeCategories.Physical;

				case 33: return AttributeCategories.Mental;
				case 34: return AttributeCategories.Mental;
				case 46: return AttributeCategories.Mental;

				case 19: return AttributeCategories.Role;
				case 6: return AttributeCategories.Role;
				case 70: return AttributeCategories.Role;
				case 71: return AttributeCategories.Role;

				case 7: return AttributeCategories.Other;
				case 8: return AttributeCategories.Other;
				case 11: return AttributeCategories.Other;
			}

			return AttributeCategories.Unknown;
		}

		public static string GetString(this AttributeValue self)
		{
			if (self.Attribute == null)
				return string.Empty;

			string name = self.Attribute.Name;
			name = name.Replace("Potency", string.Empty);

			return name + ": **" + self.Value + "**";
		}
	}
}
