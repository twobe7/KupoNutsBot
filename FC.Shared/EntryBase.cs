// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;
	using FC.Attributes;

	[Serializable]
	public abstract class EntryBase
	{
		[InspectorHidden]
		public string Id { get; set; } = string.Empty;

		[InspectorHidden]
		public DateTime? Updated { get; set; }

		public virtual void Import(EntryBase other)
		{
			this.Id = other.Id;
			this.Updated = other.Updated;
		}
	}
}
