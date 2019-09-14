// This document is intended for use by Kupo Nut Brigade developers.

namespace Lodestone.News
{
	using System;

	[Serializable]
	public class NewsItem
	{
		#pragma warning disable
		public string? id;
		public string? url;
		public string? title;
		public string? time;
		public Categories category;
		public string? image;
		public string? description;
		#pragma warning restore
	}
}
