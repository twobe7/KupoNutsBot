// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Characters
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	#pragma warning disable SA1516

	[Serializable]
	public class Data
	{
		public int ID { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
	}
}
