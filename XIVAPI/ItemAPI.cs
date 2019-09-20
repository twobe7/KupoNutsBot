// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.Threading.Tasks;

	public static class ItemAPI
	{
		public static async Task<Item> Get(ulong id)
		{
			string route = "/item/" + id;

			return await Request.Send<Item>(route);
		}

		[Serializable]
		public class Item
		{
			public int? ID;
			public string? Name;
			public string? Description;
			public string? Icon;
			public string? Url;
			public int? LevelEquip;
			public int? LevelItem;
			public bool? IsUnique;
			public bool? IsUntradable;
			public ItemCategory? ItemUICategory;
			public Parameter? BaseParam0;
			public int? BaseParamValue0;
			public Parameter? BaseParam1;
			public int? BaseParamValue1;
			public Parameter? BaseParam2;
			public int? BaseParamValue2;
			public Parameter? BaseParam3;
			public int? BaseParamValue3;
			public Parameter? BaseParam4;
			public int? BaseParamValue4;
			public Parameter? BaseParam5;
			public int? BaseParamValue5;
			public ItemSpecial? ItemSpecialBonus;
			public Parameter? BaseParamSpecial0;
			public int? BaseParamValueSpecial0;
			public Parameter? BaseParamSpecial1;
			public int? BaseParamValueSpecial1;
			public Parameter? BaseParamSpecial2;
			public int? BaseParamValueSpecial2;
			public Parameter? BaseParamSpecial3;
			public int? BaseParamValueSpecial3;
			public Parameter? BaseParamSpecial4;
			public int? BaseParamValueSpecial4;
			public Parameter? BaseParamSpecial5;
			public int? BaseParamValueSpecial5;
			public ClassJob? ClassJobCategory;
			public int DamageMag;
			public int DamagePhys;
			public int DefenseMag;
			public int DefensePhys;
			public int DelayMs;
			public int? MateriaSlotCount;
			public bool? IsAdvancedMeldingPermitted;
		}

		public class ItemCategory
		{
			public int? Id;
			public string? Icon;
			public string? Name;
		}

		public class Parameter
		{
			public string? Description;
			public string? Name;
		}

		public class ClassJob
		{
			public string? Name;
		}

		public class ItemSpecial
		{
			public string? Name;
		}
	}
}
