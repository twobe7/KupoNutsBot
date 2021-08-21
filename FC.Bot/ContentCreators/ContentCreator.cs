// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.ContentCreators
{
	using System;

	[Serializable]
	public class ContentCreator : EntryBase
	{
		public enum Type
		{
			Twitch = 0,
			Youtube = 1,
		}

		public ulong DiscordUserId { get; set; }
		public ulong DiscordGuildId { get; set; }
		public string? GuildNickName { get; set; }
		public ContentInfo? Twitch { get; set; }
		public ContentInfo? Youtube { get; set; }

		public void SetContentInfo(string identifier, Type type, string? linkId = null)
		{
			ContentInfo contentInfo = new ContentInfo(identifier, type, linkId);

			switch (type)
			{
				case Type.Twitch:
					this.Twitch = contentInfo;
					break;
				case Type.Youtube:
					this.Youtube = contentInfo;
					break;
				default:
					break;
			}
		}

		public void RemoveContentInfo(Type type)
		{
			switch (type)
			{
				case Type.Twitch:
					this.Twitch = null;
					break;
				case Type.Youtube:
					this.Youtube = null;
					break;
				default:
					break;
			}
		}

		public bool HasOtherStreams(Type type)
		{
			return type switch
			{
				Type.Twitch => this.Youtube != null,
				Type.Youtube => this.Twitch != null,
				_ => this.Twitch != null || this.Youtube != null,
			};
		}

		[Serializable]
		public class ContentInfo
		{
			public ContentInfo(string username, Type type, string? linkId = null)
			{
				this.UserName = username;
				this.LinkId = linkId ?? username;
				this.Type = type;
			}

			public string? UserName { get; set; }
			public string? LinkId { get; set; }
			public Content? LastStream { get; set; }
			public Content? LastVideo { get; set; }
			public Type Type { get; set; }
			public string? LastStreamEmbedMessageId { get; set; }

			public string Link
			{
				get
				{
					return this.Type switch
					{
						Type.Twitch => $"[{this.UserName}](https://twitch.tv/{this.UserName})",
						Type.Youtube => $"[{this.UserName}](https://www.youtube.com/channel/{this.LinkId})",
						_ => string.Empty,
					};
				}
			}

			public class Content
			{
				public Content(string? id, string? embedMessageId = null)
				{
					this.Id = id;
					this.EmbedMessageId = embedMessageId;
					this.Created = DateTime.Now;
				}

				public string? Id { get; set; }
				public DateTime Created { get; set; }
				public string? EmbedMessageId { get; set; }

				public double CreatedMinutesAgo => (DateTime.Now - this.Created).TotalMinutes;
			}
		}
	}
}
