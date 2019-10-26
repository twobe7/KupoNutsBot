// This document is intended for use by Kupo Nut Brigade developers.

namespace Lodestone.News
{
	using System;

	[Serializable]
	public class NewsItem
	{
		#pragma warning disable
		public string? id { get; set; } = string.Empty;
		public string? url { get; set; } = string.Empty;
		public string title { get; set; } = string.Empty;
		public string time { get; set; } = string.Empty;
		public Categories category { get; set; }
		public string? image { get; set; }
		public string? description { get; set; }
		public string? start { get; set; }
		public string? end { get; set; }
	}
}
