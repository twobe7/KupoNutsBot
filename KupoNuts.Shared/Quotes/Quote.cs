// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Quotes
{
	using System;

	[Serializable]
	public class Quote : EntryBase
	{
		public const int Version = 2;

		public string? Content { get; set; }

		public string? UserName { get; set; }

		public ulong? UserId { get; set; }

		public ulong? GuildId { get; set; }

		public string? DateTime { get; set; }
	}
}
