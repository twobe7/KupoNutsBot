// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.ContentCreator
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.ContentCreator;
	using FC.Data;

	using Twitch;
	using Youtube;

	[Group("content-creator", "For Streamers")]
	public class ContentCreatorService : ServiceBase
	{
		public static readonly Table<ContentCreator> ContentCreatorDatabase = new ("KupoNuts_ContentCreator", 0);

		public readonly DiscordSocketClient DiscordClient;

		public ContentCreatorService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			await ContentCreatorDatabase.Connect();

			ScheduleService.RunOnSchedule(this.Update, 10);
			await this.Update();
		}

		public override Task Shutdown()
		{
			return base.Shutdown();
		}

#if DEBUG // Commands for testing specific users
		[Command("TestStreamer", Permissions.Administrators, "Test Streamer")]
		public async Task TestStreamer(CommandMessage message, string user)
		{
			StreamerAPI.Stream stream = await StreamerAPI.GetStreams(user);

			if (!stream.IsLive)
			{
				await message.Channel.SendMessageAsync("Streamer not found or is not live");
				return;
			}

			// Send Embed
			await message.Channel.SendMessageAsync(embed: stream.ToEmbed(), messageReference: message.MessageReference);
		}

		[Command("TestStreamer", Permissions.Administrators, "Test Streamer - Aiyanya")]
		public async Task TestStreamer(CommandMessage message)
		{
			StreamerAPI.Stream stream = await StreamerAPI.GetStreams("Aiyanya");

			if (!stream.IsLive)
			{
				await message.Channel.SendMessageAsync("Streamer is not live");
				return;
			}

			// Send Embed
			await message.Channel.SendMessageAsync(embed: stream.ToEmbed(), messageReference: message.MessageReference);
		}

		[Command("TestYTUploader", Permissions.Administrators, "Test Uploader - Lacrima")]
		public async Task TestUploader(CommandMessage message)
		{
			ExploderAPI.Video video = await ExploderAPI.GetLatestVideo("UCBGZf_eNHJPCFxVxzEEWMeA");

			// Send Embed
			await message.Channel.SendMessageAsync(embed: video.ToEmbed(), messageReference: message.MessageReference);
		}
#endif

		[SlashCommand("add-twitch", "Set your twitch stream")]
		public async Task SetTwitchInformation(string username)
		{
			await this.DeferAsync(ephemeral: true);
			await this.SetContentCreator(username, ContentCreator.Type.Twitch);
		}

		[SlashCommand("remove-twitch", "Remove your set Twitch stream")]
		public async Task RemoveTwitchInformation()
		{
			await this.DeferAsync(ephemeral: true);
			await this.RemoveContentCreator(ContentCreator.Type.Twitch);
		}

		[SlashCommand("add-youtube", "Set your youtube channel using channel Id or username")]
		public async Task SetYoutubeInformation(string identifier)
		{
			await this.DeferAsync(ephemeral: true);

			(string channelId, string username) = await ExploderAPI.GetChannelInformation(identifier);
			await this.SetContentCreator(username, ContentCreator.Type.Youtube, channelId);
		}

		[SlashCommand("remove-youtube", "Remove your set Youtube information")]
		public async Task RemoveYoutubeInformation()
		{
			await this.DeferAsync(ephemeral: true);
			await this.RemoveContentCreator(ContentCreator.Type.Youtube);
		}

		[SlashCommand("list", "View current content creators")]
		public async Task ViewContentCreators()
		{
			await this.DeferAsync();

			// Load streamers
			List<ContentCreator> streamers = await ContentCreatorDatabase.LoadAll(new Dictionary<string, object>
			{
				{ "DiscordGuildId", this.Context.Guild.Id },
			});

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("Content Creators");

			// Add thumbnail
			embed.AddThumbnail(this.Context.Guild.IconUrl);

			if (streamers == null || streamers.Count == 0)
			{
				embed.Description = "No streamers found!\nUsers can add themselves with `/add-twitch` or `/add-youtube` commands";
			}
			else
			{
				StringBuilder desc = new ();

				foreach (ContentCreator streamer in streamers)
				{
					desc.Append($"{FC.Utils.DiscordMarkdownUtils.BulletPoint} {streamer.GuildNickName} | ");

					if (streamer.Twitch != null)
					{
						desc.Append($"Twitch: {streamer.Twitch.Link}");
					}

					if (streamer.Youtube != null)
					{
						if (streamer.Twitch != null)
							desc.Append(" | ");

						desc.Append($"Youtube: {streamer.Youtube.Link}");
					}

					desc.AppendLine();
				}

				embed.Description = desc.ToString();
			}

			// Send Embed
			await this.FollowupAsync(embeds: new Embed[] { embed.Build() });
		}

#if DEBUG
		[Command("CCU", Permissions.Administrators, "Posts content creators")]
#endif
		public async Task Update()
		{
			// Load streamers
			List<ContentCreator> streamers = await ContentCreatorDatabase.LoadAll();

			foreach (SocketGuild guild in this.DiscordClient.Guilds)
			{
				// Load guild settings and check if content creator channel specified
				GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guild.Id);
				if (settings.ContentCreatorChannel == null)
					continue;

				// Do not process if invalid
				if (!ulong.TryParse(settings.ContentCreatorChannel, out ulong channelId))
					continue;

				// Do not process if couldn't find channel
				SocketTextChannel contentCreatorChannel = (SocketTextChannel)this.DiscordClient.GetChannel(channelId);
				if (contentCreatorChannel == null)
					continue;

				// Channel found - load streamers
				foreach (ContentCreator streamer in streamers.Where(x => x.DiscordGuildId == guild.Id))
				{
					try
					{
						bool streamerUpdated = false;

						// Check if display name should be updated
						SocketGuildUser? user = guild.Users.FirstOrDefault(x => x.Id == streamer.DiscordGuildId);
						if (user != null)
						{
							string displayName = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;
							if (streamer.GuildNickName != displayName)
							{
								streamer.GuildNickName = displayName;
								streamerUpdated = true;
							}
						}

						// Twitch streamer
						if (streamer.Twitch != null && !string.IsNullOrWhiteSpace(streamer.Twitch.UserName))
						{
							try
							{
								StreamerAPI.Stream stream = await StreamerAPI.GetStreams(streamer.Twitch.UserName);

								// Streamer is live
								if (stream != null && stream.IsLive)
								{
									// First stream or Current stream hasn't been posted and last stream longer than 30 minutes ago
									if (streamer.Twitch.LastStream == null
										|| (stream.Id != streamer.Twitch.LastStream?.Id
											&& (stream.ParsedStartedAt - (streamer.Twitch.LastStream?.Created ?? DateTime.MinValue)).TotalMinutes > 30))
									{
										RestUserMessage message = await contentCreatorChannel.SendMessageAsync(embed: stream.ToEmbed());

										// Save streamer id
										streamer.Twitch.LastStream = new ContentCreator.ContentInfo.Content(stream.Id, message.Id.ToString());
										streamerUpdated = true;
									}
									////else
									////{
									////	if (!string.IsNullOrWhiteSpace(streamer.Twitch.LastStreamEmbedMessageId)
									////		&& ulong.TryParse(streamer.Twitch.LastStreamEmbedMessageId, out ulong messageId))
									////	{
									////		if (await contentCreatorChannel.GetMessageAsync(messageId) is RestUserMessage message)
									////			await message.UpdateAsync(); // .ModifyAsync(x => x.); // x => x.Embed = stream.ToEmbed());
									////	}
									////}

									// Reset error count if currently passing
									if (streamer.Twitch.ErrorCount != 0)
									{
										streamer.Twitch.ErrorCount = 0;
										streamerUpdated = true;
									}
								}
							}
							catch (Exception ex)
							{
								await Utils.Logger.LogExceptionToDiscordChannel(ex, "Content Creator Update", streamer.DiscordGuildId.ToString(), streamer.GuildNickName?.ToString());

								streamer.Twitch.ErrorCount++;
								if (streamer.Twitch.ErrorCount > 10)
								{
									await this.RemoveContentCreator(streamer, ContentCreator.Type.Twitch);
								}
								else
								{
									await ContentCreatorDatabase.Save(streamer);
								}
							}
						}

						// Youtube
						if (streamer.Youtube != null && !string.IsNullOrWhiteSpace(streamer.Youtube.LinkId))
						{
							try
							{
								// Check video upload
								ExploderAPI.Video latestVideo = await ExploderAPI.GetLatestVideo(streamer.Youtube.LinkId);

								// Latest Video hasn't been posted
								if (latestVideo != null && latestVideo.Id != streamer.Youtube.LastVideo?.Id)
								{
									YoutubeAPI.YoutubeVideo youtubeVideo = await YoutubeAPI.GetVideoInformation(latestVideo.Id);

									latestVideo.UploadDate = youtubeVideo.Items.FirstOrDefault()?.Snippet.PublishedAt ?? latestVideo.UploadDate;

									await contentCreatorChannel.SendMessageAsync(embed: latestVideo.ToEmbed());

									// Save last video id
									streamer.Youtube.LastVideo = new ContentCreator.ContentInfo.Content(latestVideo.Id);
									streamerUpdated = true;
								}

								// Check live stream
								ExploderAPI.Video liveStream = await ExploderAPI.GetLiveVideo(streamer.Youtube.LinkId);

								// Live stream occurring
								if (liveStream != null && liveStream.Id != streamer.Youtube.LastStream?.Id)
								{
									await contentCreatorChannel.SendMessageAsync(embed: liveStream.ToLiveEmbed());

									// Save last video id
									streamer.Youtube.LastStream = new ContentCreator.ContentInfo.Content(liveStream.Id);
									streamerUpdated = true;
								}

								// Reset error count if currently passing
								if (streamer.Youtube.ErrorCount != 0)
								{
									streamer.Youtube.ErrorCount = 0;
									streamerUpdated = true;
								}
							}
							catch (Exception ex)
							{
								await Utils.Logger.LogExceptionToDiscordChannel(ex, "Content Creator Update", streamer.DiscordGuildId.ToString(), streamer.GuildNickName?.ToString());

								streamer.Youtube.ErrorCount++;
								if (streamer.Youtube.ErrorCount > 10)
								{
									await this.RemoveContentCreator(streamer, ContentCreator.Type.Youtube);
								}
								else
								{
									await ContentCreatorDatabase.Save(streamer);
								}
							}
						}

						if (streamerUpdated)
							await ContentCreatorDatabase.Save(streamer);
					}
					catch (Exception ex)
					{
						await Utils.Logger.LogExceptionToDiscordChannel(ex, "Content Creator Update", streamer.DiscordGuildId.ToString(), streamer.GuildNickName?.ToString());
					}
				}
			}
		}

		private async Task SetContentCreator(string identifier, ContentCreator.Type type, string? linkId = null)
		{
			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			ContentCreator streamer = await ContentCreatorDatabase.LoadOrCreate(guildUser.Id.ToString());

			streamer.DiscordGuildId = this.Context.Guild.Id;
			streamer.DiscordUserId = guildUser.Id;
			streamer.GuildNickName = guildUser.GetName();

			streamer.SetContentInfo(identifier, type, linkId);

			await ContentCreatorDatabase.Save(streamer);

			// Send Embed
			await this.FollowupAsync("Added Stream Info", ephemeral: true);
		}

		private async Task RemoveContentCreator(ContentCreator.Type type)
		{
			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			ContentCreator? streamer = await ContentCreatorDatabase.Load(guildUser.Id.ToString());
			if (streamer != null)
				await this.RemoveContentCreator(streamer, type);

			// Send Embed
			await this.FollowupAsync("Removed Creator Info", ephemeral: true);
		}

		private async Task RemoveContentCreator(ContentCreator streamer, ContentCreator.Type type)
		{
			if (streamer.HasOtherStreams(type))
			{
				// Creator has more than one source - clear current and save
				streamer.RemoveContentInfo(type);
				await ContentCreatorDatabase.Save(streamer);
			}
			else
			{
				// Creator only uses current source - delete record
				await ContentCreatorDatabase.Delete(streamer);
			}
		}
	}
}
