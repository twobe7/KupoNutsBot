// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;

	[Serializable]
	public abstract class EntryBase
	{
		public string Id { get; set; } = string.Empty;

		public DateTime? Updated { get; set; }

		public virtual void Import(EntryBase other)
		{
			this.Id = other.Id;
			this.Updated = other.Updated;
		}
	}
}
