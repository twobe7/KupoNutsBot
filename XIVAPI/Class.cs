// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class Class
	{
		public string Abbreviation { get; set; } = string.Empty;
		//// public ? ClassJobCategory
		public uint ID { get; set; }
		public string Icon { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
	}
}
