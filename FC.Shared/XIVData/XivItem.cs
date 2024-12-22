// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	using System.Text.Json.Serialization;

	public class XivItem
	{
		public int Id { get; set; }
		public string Singular { get; set; } = string.Empty;
		public string Plural { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public int Icon { get; set; }

		[JsonPropertyName("Level{Item}")]
		public int ItemLevel { get; set; }
		public int Rarity { get; set; }
		public int ItemUICategory { get; set; }
		public int EquipSlotCategory { get; set; }
		public int StackSize { get; set; }
		public bool IsUnique { get; set; }
		public bool IsUntradable { get; set; }
		public bool IsIndisposable { get; set; }

		[JsonPropertyName("Price{Mid}")]
		public int PriceMid { get; set; }
		[JsonPropertyName("Price{Low}")]
		public int PriceLow { get; set; }
		public bool CanBeHq { get; set; }
		public bool IsDyeable { get; set; }
		public bool IsCrestWorthy { get; set; }

		public int ItemAction { get; set; }

		[JsonPropertyName("CastTime<s>")]
		public int CastTime { get; set; }

		[JsonPropertyName("Cooldown<s>")]
		public int Cooldown { get; set; }

		[JsonPropertyName("ClassJob{Repair}")]
		public int RepairClassJob { get; set; }

		[JsonPropertyName("Item{Repair}")]
		public int ItemRepair { get; set; }

		[JsonPropertyName("Item{Glamour}")]
		public int ItemGlamour { get; set; }

		public int Desynth { get; set; }
		public bool IsCollectable { get; set; }
		public bool AlwaysCollectable { get; set; }
		public int AetherialReduce { get; set; }

		[JsonPropertyName("Level{Equip}")]
		public int EquipLevel { get; set; }

		public int ClassJobCategory { get; set; }
		public int BaseParamModifier { get; set; }

		[JsonPropertyName("ClassJob{Use}")]
		public int ClassJobUse { get; set; }

		[JsonPropertyName("Damage{Phys}")]
		public int DamagePhysical { get; set; }

		[JsonPropertyName("Damage{Mag}")]
		public int DamageMagical { get; set; }

		[JsonPropertyName("Delay<ms>")]
		public int Delay { get; set; }
		public int BlockRate { get; set; }
		public int Block { get; set; }

		[JsonPropertyName("Defense{Phys}")]
		public int DefensePhysical { get; set; }
		[JsonPropertyName("Defense{Mag}")]
		public int DefenseMagical { get; set; }

		[JsonPropertyName("BaseParam[0]")]
		public int BaseParam0 { get; set; }

		[JsonPropertyName("BaseParamValue[0]")]
		public int BaseParamValue0 { get; set; }

		[JsonPropertyName("BaseParam[1]")]
		public int BaseParam1 { get; set; }

		[JsonPropertyName("BaseParamValue[1]")]
		public int BaseParamValue1 { get; set; }

		[JsonPropertyName("BaseParam[2]")]
		public int BaseParam2 { get; set; }

		[JsonPropertyName("BaseParamValue[2]")]
		public int BaseParamValue2 { get; set; }

		[JsonPropertyName("BaseParam[3]")]
		public int BaseParam3 { get; set; }

		[JsonPropertyName("BaseParamValue[3]")]
		public int BaseParamValue3 { get; set; }

		[JsonPropertyName("BaseParam[4]")]
		public int BaseParam4 { get; set; }

		[JsonPropertyName("BaseParamValue[4]")]
		public int BaseParamValue4 { get; set; }

		[JsonPropertyName("BaseParam[5]")]
		public int BaseParam5 { get; set; }

		[JsonPropertyName("BaseParamValue[5]")]
		public int BaseParamValue5 { get; set; }

		public int ItemSpecialBonus { get; set; }

		[JsonPropertyName("ItemSpecialBonus{Param}")]
		public int ItemSpecialBonusParam { get; set; }

		[JsonPropertyName("BaseParam{Special}[0]")]
		public int BaseParamSpecial0 { get; set; }
		[JsonPropertyName("BaseParamValue{Special}[0]")]
		public int BaseParamValueSpecial0 { get; set; }

		[JsonPropertyName("BaseParam{Special}[1]")]
		public int BaseParamSpecial1 { get; set; }
		[JsonPropertyName("BaseParamValue{Special}[1]")]
		public int BaseParamValueSpecial1 { get; set; }

		[JsonPropertyName("BaseParam{Special}[2]")]
		public int BaseParamSpecial2 { get; set; }
		[JsonPropertyName("BaseParamValue{Special}[2]")]
		public int BaseParamValueSpecial2 { get; set; }

		[JsonPropertyName("BaseParam{Special}[3]")]
		public int BaseParamSpecial3 { get; set; }
		[JsonPropertyName("BaseParamValue{Special}[3]")]
		public int BaseParamValueSpecial3 { get; set; }

		[JsonPropertyName("BaseParam{Special}[4]")]
		public int BaseParamSpecial4 { get; set; }
		[JsonPropertyName("BaseParamValue{Special}[4]")]
		public int BaseParamValueSpecial4 { get; set; }

		[JsonPropertyName("BaseParam{Special}[5]")]
		public int BaseParamSpecial5 { get; set; }
		[JsonPropertyName("BaseParamValue{Special}[5]")]
		public int BaseParamValueSpecial5 { get; set; }

		public int MaterializeType { get; set; }
		public int MateriaSlotCount { get; set; }

		public bool IsAdvancedMeldingPermitted { get; set; }
		public bool IsPvP { get; set; }
		public bool IsGlamourous { get; set; }

		public int SubStatCategory { get; set; }

		public string IconString => this.Icon.ToString();
		public string IconBasePath => $"0{this.IconString[..2]}000";
		public string IconPadded => this.IconString.PadLeft(6, '0');
		public string IconFullPath => $"https://xivapi.com/i/{this.IconBasePath}/{this.IconPadded}.png";
	}
}
