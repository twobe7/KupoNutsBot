// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	[Serializable]
	#pragma warning disable SA1516
	public class Action : ResponseBase
	{
		public int ID { get; set; } = 0;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;

		public int ClassJobLevel { get; set; }
		public int Cast100ms { get; set; }

		public decimal CastSeconds
		{
			get
			{
				return this.Cast100ms / 10M;
			}
		}

		public int Recast100ms { get; set; }

		public decimal RecastSeconds
		{
			get
			{
				return this.Recast100ms / 10M;
			}
		}

		public ClassJob? ClassJob { get; set; }
		public JobCategory? ClassJobCategory { get; set; }
		public GameContentLink? ContentLink { get; set; }

		[Serializable]
		public class JobCategory
		{
			public string Name { get; set; } = string.Empty;
			public bool ACN { get; set; }
			public bool ADV { get; set; }
			public bool ALC { get; set; }
			public bool ARC { get; set; }
			public bool ARM { get; set; }
			public bool AST { get; set; }
			public bool BLM { get; set; }
			public bool BLU { get; set; }
			public bool BRD { get; set; }
			public bool BSM { get; set; }
			public bool BTN { get; set; }
			public bool CNJ { get; set; }
			public bool CRP { get; set; }
			public bool CUL { get; set; }
			public bool DNC { get; set; }
			public bool DRG { get; set; }
			public bool DRK { get; set; }
			public bool FSH { get; set; }
			public bool GLA { get; set; }
			public bool GNB { get; set; }
			public bool GSM { get; set; }
			public bool LNC { get; set; }
			public bool LTW { get; set; }
			public bool MCH { get; set; }
			public bool MIN { get; set; }
			public bool MNK { get; set; }
			public bool MRD { get; set; }
			public bool NIN { get; set; }
			public bool PGL { get; set; }
			public bool PLD { get; set; }
			public bool RDM { get; set; }
			public bool ROG { get; set; }
			public bool SAM { get; set; }
			public bool SCH { get; set; }
			public bool SMN { get; set; }
			public bool THM { get; set; }
			public bool WAR { get; set; }
			public bool WHM { get; set; }
			public bool WVR { get; set; }
		}
	}
}
