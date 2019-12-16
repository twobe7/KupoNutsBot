// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class SundayFundayEvent : EntryBase
	{
		public string Name { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public string Reaction { get; set; } = string.Empty;
	}
}
