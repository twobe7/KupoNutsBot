// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Quotes
{
	using System;

	[Serializable]
	public class Quote : EntryBase
	{
		public const int Version = 2;

		public string Content { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string MessageLink { get; set; } = string.Empty;
		public ulong UserId { get; set; } = 0;
		public ulong GuildId { get; set; } = 0;
		public string? DateTime { get; set; }
		public int QuoteId { get; set; } = -1;

		public string GetQuoteDescription()
		{
			System.Text.StringBuilder desc = new System.Text.StringBuilder();

			desc.AppendLine($"\"*{this.Content}*\"");

			if (!string.IsNullOrWhiteSpace(this.MessageLink))
			{
				desc.AppendLine();
				desc.AppendLine($"[Original Post]({this.MessageLink})");
			}

			return desc.ToString();
		}
	}
}
