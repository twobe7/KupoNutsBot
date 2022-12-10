// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class Data
	{
		public Data()
		{
		}

		public Data(int? id = null, string? name = null, string? icon = null, string? url = null)
		{
			if (id.HasValue)
				this.ID = id.Value;
			if (name != null)
				this.Name = name;
			if (icon != null)
				this.Icon = icon;
			if (url != null)
				this.Url = url;
		}

		public int ID { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
	}
}
