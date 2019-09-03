// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;

	[Serializable]
	public class Channel : EntryBase
	{
		public string? DiscordId;
		public string? Name;
		public Types Type;

		public enum Types
		{
			Unknown,
			Text,
			Voice,
		}
	}
}
