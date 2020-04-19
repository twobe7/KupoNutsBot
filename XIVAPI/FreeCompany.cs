// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class FreeCompany
	{
		public uint ActiveMemberCount { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Tag { get; set; } = string.Empty;
		public string Slogan { get; set; } = string.Empty;
		public List<string> Crest { get; set; } = new List<string>();
	}
}
