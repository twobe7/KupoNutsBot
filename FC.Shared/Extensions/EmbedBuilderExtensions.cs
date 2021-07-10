// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace System.Collections.Generic
{
	using Discord;

	public static class EmbedBuilderExtensions
	{
		/// <summary>
		/// Add Thumbnail Url to EmbedBuilder, replaces jpg with webp.
		/// </summary>
		/// <param name="self">This EmbedBuilder object.</param>
		/// <param name="iconUrl">The Url of Icon to add.</param>
		public static void AddThumbnail(this EmbedBuilder self, string iconUrl)
		{
			if (iconUrl.EndsWith(".jpg"))
				iconUrl = iconUrl.Replace(".jpg", ".webp?size=1024");

			self.ThumbnailUrl = iconUrl;
		}
	}
}