// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FFXIVCollect
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public static class MountAPI
	{
		public static async Task<Mount> Get(ulong id)
		{
			return await Request.Send<Mount>("/mounts/" + id);
		}

		[Serializable]
		public class Mount
		{
			public ulong? ID { get; set; }
			public string Name { get; set; } = string.Empty;
			public string Description { get; set; } = string.Empty;
			public string Enhanced_Description { get; set; } = string.Empty;
			public string Tooltip { get; set; } = string.Empty;
			public bool Flying { get; set; } = false;
			public string Movement { get; set; } = "N/A";
			public int Seats { get; set; }
			public int Order { get; set; }
			public string Patch { get; set; } = "N/A";
			public string Owned { get; set; } = "N/A";
			public string? Image { get; set; }
			public string? Icon { get; set; }
			public List<Source> Sources { get; set; } = new List<Source>();

			public class Source
			{
				public string? Type { get; set; }
				public string? Text { get; set; }
				public string? Related_Type { get; set; }
				////public int? Related_Id { get; set; }
			}
		}
	}
}
