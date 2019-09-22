// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;

	public class FashionReportEntry : EntryBase
	{
		public DateTime? Time { get; set; }

		public string? Content { get; set; }

		public string? ImageUrl { get; set; }

		public string? Author { get; set; }

		public string? AuthorImageUrl { get; set; }
	}
}
