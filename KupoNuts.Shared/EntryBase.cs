// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;

	[Serializable]
	public abstract class EntryBase
	{
		public string? Id;
		public DateTime? Updated;

		public virtual void Import(EntryBase other)
		{
			this.Id = other.Id;
			this.Updated = other.Updated;
		}
	}
}
