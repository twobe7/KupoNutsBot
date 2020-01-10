// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
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
