// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Serializable]
	#pragma warning disable SA1516
	public class Item : ResponseBase
	{
		public int ID { get; set; } = 0;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
		public bool AetherialReduce { get; set; }
		public int MaterializeType { get; set; }
		public int LevelEquip { get; set; }
		public int LevelItem { get; set; }
		public bool IsUnique { get; set; }
		public bool IsUntradable { get; set; }
		public bool IsDyeable { get; set; }
		public bool IsGlamourous { get; set; }
		public GameContentLink? GameContentLinks { get; set; }
		public ItemCategory? ItemUICategory { get; set; }
		public Parameter? BaseParam0 { get; set; }
		public int BaseParamValue0 { get; set; }
		public Parameter? BaseParam1 { get; set; }
		public int BaseParamValue1 { get; set; }
		public Parameter? BaseParam2 { get; set; }
		public int BaseParamValue2 { get; set; }
		public Parameter? BaseParam3 { get; set; }
		public int BaseParamValue3 { get; set; }
		public Parameter? BaseParam4 { get; set; }
		public int BaseParamValue4 { get; set; }
		public Parameter? BaseParam5 { get; set; }
		public int BaseParamValue5 { get; set; }
		public ItemSpecial? ItemSpecialBonus { get; set; }
		public Parameter? BaseParamSpecial0 { get; set; }
		public int BaseParamValueSpecial0 { get; set; }
		public Parameter? BaseParamSpecial1 { get; set; }
		public int BaseParamValueSpecial1 { get; set; }
		public Parameter? BaseParamSpecial2 { get; set; }
		public int BaseParamValueSpecial2 { get; set; }
		public Parameter? BaseParamSpecial3 { get; set; }
		public int BaseParamValueSpecial3 { get; set; }
		public Parameter? BaseParamSpecial4 { get; set; }
		public int BaseParamValueSpecial4 { get; set; }
		public Parameter? BaseParamSpecial5 { get; set; }
		public int BaseParamValueSpecial5 { get; set; }
		public ClassJob? ClassJobCategory { get; set; }
		public int DamageMag { get; set; }
		public int DamagePhys { get; set; }
		public int DefenseMag { get; set; }
		public int DefensePhys { get; set; }
		public int DelayMs { get; set; }
		public int MateriaSlotCount { get; set; }
		public bool IsAdvancedMeldingPermitted { get; set; }
		public SalvageInfo? Salvage { get; set; }
		public StatBonuses? Bonuses { get; set; }

		[Serializable]
		public class ItemCategory
		{
			public int Id { get; set; }
			public string Icon { get; set; } = string.Empty;
			public string Name { get; set; } = string.Empty;
		}

		[Serializable]
		public class Parameter
		{
			public string Description { get; set; } = string.Empty;
			public string Name { get; set; } = string.Empty;
		}

		[Serializable]
		public class ClassJob
		{
			public string Name { get; set; } = string.Empty;
		}

		[Serializable]
		public class ItemSpecial
		{
			public string Name { get; set; } = string.Empty;
		}

		[Serializable]
		public class SalvageInfo
		{
			public int ID { get; set; }
			public int OptimalSkill { get; set; }
		}

		[Serializable]
		public class GameContentLink
		{
			public Link? FurnitureCatalogItemList { get; set; }
			public Link? HousingFurniture { get; set; }
			public Link? Recipe { get; set; }

			[Serializable]
			public class Link
			{
				public List<int> Item { get; set; } = new List<int>();
				public List<int> ItemResult { get; set; } = new List<int>();
			}
		}

		[Serializable]
		public class StatBonuses
		{
			public StatBonus? Piety { get; set; }
			public StatBonus? SpellSpeed { get; set; }
			public StatBonus? Vitality { get; set; }
			public StatBonus? Tenacity { get; set; }
			public StatBonus? Determination { get; set; }
			public StatBonus? DirectHit { get; set; }
			public StatBonus? SkillSpeed { get; set; }
			public StatBonus? CriticalHit { get; set; }
			public StatBonus? CP { get; set; }
			public StatBonus? Control { get; set; }
			public StatBonus? Perception { get; set; }
			public StatBonus? GP { get; set; }
			public StatBonus? Gathering { get; set; }
			public StatBonus? Craftsmanship { get; set; }
		}

		[Serializable]
		public class StatBonus
		{
			public int Id;
			public int Max;
			public int MaxHq;
			public bool Relative;
			public int Value;
			public int ValueHq;
		}
	}
}
