// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Twitch
{
	using Discord;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public class StreamerAPI
	{
		public static async Task<Stream> GetStreams(string username)
		{
			StreamResponse response = await Request.Send<StreamResponse>(username);

			if (response.Data == null || response.Data.Count == 0)
				return new Stream();

			return response.Data[0];
		}

		[Serializable]
		public class StreamResponse : ResponseBase
		{
			public List<Stream>? Data { get; set; }
			public object? Pagination { get; set; }
		}

		public class Stream
		{
			public string? Id;
			public string? UserId { get; set; }
			public string? UserLogin { get; set; }
			public string? UserName { get; set; }
			public string? GameId { get; set; }
			public string? GameName { get; set; }
			public string? Type { get; set; }
			public string? Title { get; set; }
			public int? ViewerCount { get; set; }
			public string? StartedAt { get; set; }
			public string? Language { get; set; }
			public string? ThumbnailUrl { get; set; }
			public string[]? TagIds { get; set; }
			public bool? IsMature { get; set; }

			public DateTime ParsedStartedAt => DateTime.TryParse(this.StartedAt, out DateTime result) ? result : DateTime.Now;

			public bool IsLive => !string.IsNullOrWhiteSpace(this.Id);

			public string ChannelName => $"[{this.UserName}](https://twitch.tv/{this.UserName})";

			public string GetThumbnailUrl(uint width, uint height)
			{
				return this.ThumbnailUrl.Replace("{width}", width.ToString()).Replace("{height}", height.ToString()) + $"?v={DateTimeOffset.Now.ToUnixTimeSeconds()}";
			}

			public Embed ToEmbed(uint width = 900, uint height = 600, string username = null)
			{
				EmbedBuilder embed = new EmbedBuilder
				{
					Color = Color.DarkPurple,
					ThumbnailUrl = "https://image.flaticon.com/icons/png/256/2111/2111668.png",
					Title = $"Now Streaming: {username ?? this.UserName}",
					Description = this.Title,
					ImageUrl = this.GetThumbnailUrl(width, height),
				};

				embed.AddField("Channel", this.ChannelName, true);
				embed.AddField("Viewer Count", this.ViewerCount, true);

				embed.WithFooter($"Playing: {this.GameName}");

				return embed.Build();
			}
		}
	}
}
