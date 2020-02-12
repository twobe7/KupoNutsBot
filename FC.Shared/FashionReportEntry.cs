// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;

	public class FashionReportEntry : EntryBase
	{
		public DateTime Time { get; set; }

		public string Content { get; set; } = string.Empty;

		public string ImageUrl { get; set; } = string.Empty;

		public string Author { get; set; } = string.Empty;

		public string AuthorImageUrl { get; set; } = string.Empty;
	}
}
