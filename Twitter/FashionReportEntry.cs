// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Twitter
{
	using System;
	using System.Collections.Generic;
	using System.Text.Json.Serialization;
	using System.Text.RegularExpressions;
	using Discord;
	using FC;

	public partial class FashionReportEntry : EntryBase
	{
		[JsonPropertyName("created_at")]
		public DateTime Time { get; set; }

		[JsonPropertyName("text")]
		public string Content { get; set; } = string.Empty;

		public string ImageUrl { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the name of the Author
		/// Default to Kaiyoko as the author of Fashion Report posts.
		/// </summary>
		public string Author { get; set; } = "KaiyokoStar";

		/// <summary>
		/// Gets or sets the image URL of the Author of the Tweet
		/// Default to Kaiyoko as the author of Fashion Report posts.
		/// </summary>
		public string AuthorImageUrl { get; set; } = "https://x.com/KaiyokoStar/photo";

		public Attachment? Attachments { get; set; }

		public Embed GetEmbed()
		{
			EmbedBuilder builder = new()
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = this.AuthorImageUrl,
					Name = this.Author,
				},
				ImageUrl = this.ImageUrl,
				Description = this.Content,
				Color = Color.Magenta,
				Footer = new EmbedFooterBuilder
				{
					IconUrl = "https://image.flaticon.com/icons/png/512/733/733579.png",
					Text = $"@{this.Author} - Posted {(DateTime.Now - this.Time).ToMediumString()} ago",
				},
			};
			return builder.Build();
		}

		public class Attachment
		{
			public List<string> MediaKeys { get; set; } = [];
		}
	}
}
