// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;

	[Serializable]
	public class Settings
	{
		public string? Token { get; set; }

		public string? LogChannel { get; set; }

		public string? StatusChannel { get; set; }

		public string? CalendarChannel { get; set; }
	}
}
