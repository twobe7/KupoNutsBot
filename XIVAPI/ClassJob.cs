// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Characters
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	#pragma warning disable SA1516

	[Serializable]
	public class ClassJob
	{
		public Class? Class { get; set; }
		public ulong ExpLevel { get; set; } = 0;
		public ulong ExpLevelMax { get; set; } = 0;
		public ulong ExpLevelTogo { get; set; } = 0;
		public bool IsSpecialised { get; set; } = false;
		public Class? Job { get; set; }
		public int Level { get; set; } = 0;
		public string Name { get; set; } = string.Empty;
	}
}
