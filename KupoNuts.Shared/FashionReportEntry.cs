// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
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
