// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
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
