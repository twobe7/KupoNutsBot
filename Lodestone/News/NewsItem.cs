// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Lodestone.News
{
	using System;

	[Serializable]
	public class NewsItem
	{
		public string? Id { get; set; } = string.Empty;
		public string? Url { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Time { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public string? Image { get; set; }
		public string? Description { get; set; }
		public string? Start { get; set; }
		public string? End { get; set; }
	}
}
