// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Youtube
{
	using Discord;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using YoutubeExplode;

	public class ExploderAPI
	{
		public static async Task<(string channelId, string username)> GetChannelInformation(string identifier)
		{
			YoutubeClient youtube = new YoutubeClient();

			try
			{
				// Get by channel id
				YoutubeExplode.Channels.Channel channel = await youtube.Channels.GetAsync(identifier);
				return (channel.Id, channel.Title);
			}
			catch (Exception)
			{
				try
				{
					// Get by username
					YoutubeExplode.Channels.Channel channel = await youtube.Channels.GetByUserAsync(identifier);
					return (channel.Id, channel.Title);
				}
				catch (Exception)
				{
					// Cannot find by channel id or user, return identifier as both results
					return (identifier, identifier);
				}
			}
		}

		public static async Task<Video> GetLatestVideo(string channelId)
		{
			YoutubeClient youtube = new YoutubeClient();

			try
			{
				IAsyncEnumerable<YoutubeExplode.Playlists.PlaylistVideo> uploads = youtube.Channels.GetUploadsAsync(channelId);

				YoutubeExplode.Videos.VideoId videoId;
				await foreach (YoutubeExplode.Playlists.PlaylistVideo uploadedVideo in uploads)
				{
					videoId = uploadedVideo.Id;
					break;
				}

				// Get video information
				return await GetVideoInformation(videoId);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static async Task<Video> GetVideoInformation(string videoId)
		{
			YoutubeClient youtube = new YoutubeClient();

			try
			{
				// Get video information
				YoutubeExplode.Videos.Video video = await youtube.Videos.GetAsync(videoId);
				return new Video(video);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static async Task<Video> GetLiveVideo(string channelId)
		{
			string videoId = await Request.GetLiveVideoId(channelId);

			if (!string.IsNullOrWhiteSpace(videoId))
			{
				return await GetVideoInformation(videoId);
			}

			return null;
		}

		public class Video
		{
			public Video(YoutubeExplode.Videos.Video video)
			{
				this.Id = video.Id.ToString();
				this.ChannelId = video.Author.ChannelId;
				this.Author = video.Author.Title;
				this.Url = video.Url;
				this.Title = video.Title;
				this.Description = video.Description;
				this.UploadDate = video.UploadDate.DateTime;
				this.Duration = video.Duration;
				this.ThumbnailUrl = video.Thumbnails.GetFirst()?.Url;
			}

			public string Id { get; set; }
			public string ChannelId { get; set; }
			public string Author { get; set; }
			public string Url { get; set; }
			public string Title { get; set; }
			public string Description { get; set; }
			public DateTime UploadDate { get; set; }
			public TimeSpan? Duration { get; set; }
			public string? ThumbnailUrl { get; set; }

			public string TitleUrl => $"[{this.Title}]({this.Url})";
			public string ChannelUrl => $"[{this.Author}](https://www.youtube.com/channel/{this.ChannelId})";
			public string UploadedAgo => (DateTime.Now - this.UploadDate).ToMediumString() + " ago";

			public Embed ToEmbed(uint width = 400, uint height = 250)
			{
				// Build embed
				EmbedBuilder embed = new EmbedBuilder
				{
					ThumbnailUrl = "https://image.flaticon.com/icons/png/512/1384/1384060.png",
					Title = this.Title,
					//Description = FormatDescription(),
					ImageUrl = GetThumbnailUrl(),
					Url = this.Url,
				};

				embed.AddField("Uploaded By", this.ChannelUrl);
				embed.AddField("Description", FormatDescription());

				embed.WithFooter($"Video Uploaded: {this.UploadedAgo}");

				return embed.Build();
			}

			public Embed ToLiveEmbed(uint width = 400, uint height = 250)
			{
				// Build embed
				EmbedBuilder embed = new EmbedBuilder
				{
					ThumbnailUrl = "https://image.flaticon.com/icons/png/512/1384/1384060.png",
					Title = this.Title,
					//Description = FormatDescription(),
					ImageUrl = GetThumbnailUrl(),
					Url = this.Url,
				};

				embed.AddField("Streamer", this.ChannelUrl);
				embed.AddField("Description", FormatDescription());

				embed.WithFooter($"Live Stream Started: {this.UploadedAgo}");

				return embed.Build();
			}

			private string GetThumbnailUrl()
			{
				int idx = ThumbnailUrl.IndexOf(".jpg");
				if (idx != -1)
					return ThumbnailUrl.Substring(0, idx + 4);

				return ThumbnailUrl;
			}

			private string FormatDescription()
			{
				return Description.Replace("\n\n\n\n", "\n\n").Truncate(512) + "\n";
			}
		}
	}
}
