// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

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
