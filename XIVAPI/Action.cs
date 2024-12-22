// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Text.Json.Serialization;
	using FC.API;
	using FC.Serialization;

	[Serializable]
	public class Action : ResponseBase
	{
		public int ID { get; set; } = 0;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;

		public int ClassJobLevel { get; set; }
		public int Cast100ms { get; set; }

		public decimal CastSeconds => this.Cast100ms / 10M;

		public int Recast100ms { get; set; }

		public decimal RecastSeconds => this.Recast100ms / 10M;

		public ClassJob? ClassJob { get; set; }
		public JobCategory? ClassJobCategory { get; set; }
		public GameContentLink? ContentLink { get; set; }

		[Serializable]
		public class JobCategory
		{
			public string Name { get; set; } = string.Empty;
			[JsonConverter(typeof(BoolConvertor))]
			public bool ACN { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool ADV { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool ALC { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool ARC { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool ARM { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool AST { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool BLM { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool BLU { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool BRD { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool BSM { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool BTN { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool CNJ { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool CRP { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool CUL { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool DNC { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool DRG { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool DRK { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool FSH { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool GLA { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool GNB { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool GSM { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool LNC { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool LTW { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool MCH { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool MIN { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool MNK { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool MRD { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool NIN { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool PGL { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool PLD { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool RDM { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool ROG { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool SAM { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool SCH { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool SMN { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool THM { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool WAR { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool WHM { get; set; }
			[JsonConverter(typeof(BoolConvertor))]
			public bool WVR { get; set; }
		}
	}
}
