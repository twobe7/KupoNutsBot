// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	using Discord;
	using Newtonsoft.Json;

	public class XivItem
	{
		public int Id { get; set; }
		public string Singular { get; set; } = string.Empty;
		public string Plural { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;

		[JsonProperty("Level{Item}")]
		public int ItemLevel { get; set; }
		public int Rarity { get; set; }
		public int ItemUICategory { get; set; }
		public int EquipSlotCategory { get; set; }
		public int StackSize { get; set; }
		public bool IsUnique { get; set; }
		public bool IsUntradable { get; set; }
		public bool IsIndisposable { get; set; }

		[JsonProperty("Price{Mid}")]
		public int PriceMid { get; set; }
		[JsonProperty("Price{Low}")]
		public int PriceLow { get; set; }
		public bool CanBeHq { get; set; }
		public bool IsDyeable { get; set; }
		public bool IsCrestWorthy { get; set; }

		public int ItemAction { get; set; }

		[JsonProperty("CastTime<s>")]
		public int CastTime { get; set; }

		[JsonProperty("Cooldown<s>")]
		public int Cooldown { get; set; }

		[JsonProperty("ClassJob{Repair}")]
		public int RepairClassJob { get; set; }

		[JsonProperty("Item{Repair}")]
		public int ItemRepair { get; set; }

		[JsonProperty("Item{Glamour}")]
		public int ItemGlamour { get; set; }

		public int Desynth { get; set; }
		public bool IsCollectable { get; set; }
		public bool AlwaysCollectable { get; set; }
		public int AetherialReduce { get; set; }

		[JsonProperty("Level{Equip}")]
		public int EquipLevel { get; set; }

		public int ClassJobCategory { get; set; }
		public int BaseParamModifier { get; set; }

		[JsonProperty("ClassJob{Use}")]
		public int ClassJobUse { get; set; }

		[JsonProperty("Damage{Phys}")]
		public int DamagePhysical { get; set; }

		[JsonProperty("Damage{Mag}")]
		public int DamageMagical { get; set; }

		[JsonProperty("Delay<ms>")]
		public int Delay { get; set; }
		public int BlockRate { get; set; }
		public int Block { get; set; }

		[JsonProperty("Defense{Phys}")]
		public int DefensePhysical { get; set; }
		[JsonProperty("Defense{Mag}")]
		public int DefenseMagical { get; set; }

		[JsonProperty("BaseParam[0]")]
		public int BaseParam0 { get; set; }

		[JsonProperty("BaseParamValue[0]")]
		public int BaseParamValue0 { get; set; }

		[JsonProperty("BaseParam[1]")]
		public int BaseParam1 { get; set; }

		[JsonProperty("BaseParamValue[1]")]
		public int BaseParamValue1 { get; set; }

		[JsonProperty("BaseParam[2]")]
		public int BaseParam2 { get; set; }

		[JsonProperty("BaseParamValue[2]")]
		public int BaseParamValue2 { get; set; }

		[JsonProperty("BaseParam[3]")]
		public int BaseParam3 { get; set; }

		[JsonProperty("BaseParamValue[3]")]
		public int BaseParamValue3 { get; set; }

		[JsonProperty("BaseParam[4]")]
		public int BaseParam4 { get; set; }

		[JsonProperty("BaseParamValue[4]")]
		public int BaseParamValue4 { get; set; }

		[JsonProperty("BaseParam[5]")]
		public int BaseParam5 { get; set; }

		[JsonProperty("BaseParamValue[5]")]
		public int BaseParamValue5 { get; set; }

		public int ItemSpecialBonus { get; set; }

		[JsonProperty("ItemSpecialBonus{Param}")]
		public int ItemSpecialBonusParam { get; set; }

		[JsonProperty("BaseParam{Special}[0]")]
		public int BaseParamSpecial0 { get; set; }
		[JsonProperty("BaseParamValue{Special}[0]")]
		public int BaseParamValueSpecial0 { get; set; }

		[JsonProperty("BaseParam{Special}[1]")]
		public int BaseParamSpecial1 { get; set; }
		[JsonProperty("BaseParamValue{Special}[1]")]
		public int BaseParamValueSpecial1 { get; set; }

		[JsonProperty("BaseParam{Special}[2]")]
		public int BaseParamSpecial2 { get; set; }
		[JsonProperty("BaseParamValue{Special}[2]")]
		public int BaseParamValueSpecial2 { get; set; }

		[JsonProperty("BaseParam{Special}[3]")]
		public int BaseParamSpecial3 { get; set; }
		[JsonProperty("BaseParamValue{Special}[3]")]
		public int BaseParamValueSpecial3 { get; set; }

		[JsonProperty("BaseParam{Special}[4]")]
		public int BaseParamSpecial4 { get; set; }
		[JsonProperty("BaseParamValue{Special}[4]")]
		public int BaseParamValueSpecial4 { get; set; }

		[JsonProperty("BaseParam{Special}[5]")]
		public int BaseParamSpecial5 { get; set; }
		[JsonProperty("BaseParamValue{Special}[5]")]
		public int BaseParamValueSpecial5 { get; set; }

		public int MaterializeType { get; set; }
		public int MateriaSlotCount { get; set; }

		public bool IsAdvancedMeldingPermitted { get; set; }
		public bool IsPvP { get; set; }
		public bool IsGlamourous { get; set; }

		public int SubStatCategory { get; set; }

		public string IconBasePath => $"0{this.Icon[..2]}000";
		public string IconPadded => this.Icon.PadLeft(6, '0');
		public string IconFullPath => $"https://xivapi.com/i/{this.IconBasePath}/{this.IconPadded}.png";
	}
}
