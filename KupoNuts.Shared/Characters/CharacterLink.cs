// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Characters
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	#pragma warning disable SA1516

	[Serializable]
	public class CharacterLink : EntryBase
	{
		public ulong UserId { get; set; }
		public ulong GuildId { get; set; }
		public uint CharacterId { get; set; }
	}
}
