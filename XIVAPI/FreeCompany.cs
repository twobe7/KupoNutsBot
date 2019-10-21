// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	#pragma warning disable SA1516

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
