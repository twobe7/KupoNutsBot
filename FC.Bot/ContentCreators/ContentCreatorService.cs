// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.ContentCreators
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Data;
	using Twitch;
	using Youtube;

	public class ContentCreatorService : ServiceBase
	{
		public static Table<ContentCreator> ContentCreatorDatabase = new Table<ContentCreator>("KupoNuts_ContentCreator", 0);

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
			ExploderAPI.Video video = await ExploderAPI.GetLatestVideo("UCk_PCfHt9qefbsbSJMsKpRg");

			// Send Embed
			await message.Channel.SendMessageAsync(embed: video.ToEmbed(), messageReference: message.MessageReference);
		}
#endif

		[Command("ICreatorTwitch", Permissions.Everyone, "Set your twitch stream", CommandCategory.ContentCreators, showWait: false)]
		public async Task SetTwitchInformation(CommandMessage message, string username)
		{
			await this.SetContentCreator(message, username, ContentCreator.Type.Twitch);
		}

		[Command("RemoveCreatorTwitch", Permissions.Everyone, "Remove your set Twitch stream", CommandCategory.ContentCreators)]
		public async Task RemoveTwitchInformation(CommandMessage message)
		{
			await this.RemoveContentCreator(message, ContentCreator.Type.Twitch);
		}

		[Command("ICreatorYoutube", Permissions.Everyone, "Set your youtube channel using channel Id or username", showWait: false)]
		public async Task SetYoutubeInformation(CommandMessage message, string identifier)
		{
			(string channelId, string username) = await ExploderAPI.GetChannelInformation(identifier);
			await this.SetContentCreator(message, username, ContentCreator.Type.Youtube, channelId);
		}

		[Command("RemoveCreatorYoutube", Permissions.Everyone, "Remove your set Youtube information", CommandCategory.ContentCreators)]
		public async Task RemoveYoutubeInformation(CommandMessage message)
		{
			await this.RemoveContentCreator(message, ContentCreator.Type.Youtube);
		}

		[Command("CC", Permissions.Everyone, "View current content creators", CommandCategory.ContentCreators, "ContentCreators")]
		[Command("ContentCreators", Permissions.Everyone, "View current content creators", CommandCategory.ContentCreators)]
		public async Task ViewContentCreators(CommandMessage message)
		{
			// Load streamers
			List<ContentCreator> streamers = await ContentCreatorDatabase.LoadAll(new Dictionary<string, object> { { "DiscordGuildId", message.Guild.Id } });

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("Content Creators");

			// Add thumbnail
			embed.AddThumbnail(message.Guild.IconUrl);

			if (streamers == null || streamers.Count == 0)
			{
				// TODO: Add YT when implemented
				string prefix = CommandsService.GetPrefix(message.Guild.Id);
				embed.Description = $"No streamers found!\nUsers can add themselves with {prefix}IStreamTwitch command";
			}
			else
			{
				StringBuilder desc = new StringBuilder();

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
			await message.Channel.SendMessageAsync(embed: embed.Build(), messageReference: message.MessageReference);
		}

#if DEBUG
		[Command("CCU", Permissions.Administrators, "Posts content creators")]
#endif
		public async Task Update()
		{
			// Load streamers
			List<ContentCreator> streamers = await ContentCreatorDatabase.LoadAll();

			foreach (SocketGuild guild in Program.DiscordClient.Guilds)
			{
				// Load guild settings and check if content creator channel specified
				GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guild.Id);
				if (settings.ContentCreatorChannel == null)
					continue;

				// Do not process if invalid
				if (!ulong.TryParse(settings.ContentCreatorChannel, out ulong channelId))
					continue;

				// Do not process if couldn't find channel
				SocketTextChannel contentCreatorChannel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);
				if (contentCreatorChannel == null)
					continue;

				// Channel found - load streamers
				foreach (ContentCreator streamer in streamers.Where(x => x.DiscordGuildId == guild.Id))
				{
					bool streamerUpdated = false;

					// Twitch streamer
					if (streamer.Twitch != null && !string.IsNullOrWhiteSpace(streamer.Twitch.UserName))
					{
						StreamerAPI.Stream stream = await StreamerAPI.GetStreams(streamer.Twitch.UserName);

						// Streamer is live and current stream hasn't been posted
						if (stream.IsLive)
						{
							if (stream.Id != streamer.Twitch.LastStreamId)
							{
								RestUserMessage message = await contentCreatorChannel.SendMessageAsync(embed: stream.ToEmbed());

								// Save streamer id
								streamer.Twitch.LastStreamId = stream.Id;
								streamer.Twitch.LastStreamEmbedMessageId = message.Id.ToString();
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
						}
					}

					// Youtube
					if (streamer.Youtube != null && !string.IsNullOrWhiteSpace(streamer.Youtube.LinkId))
					{
						// Check video upload
						ExploderAPI.Video latestVideo = await ExploderAPI.GetLatestVideo(streamer.Youtube.LinkId);

						// Latest Video hasn't been posted
						if (latestVideo != null && latestVideo.Id != streamer.Youtube.LastVideoId)
						{
							await contentCreatorChannel.SendMessageAsync(embed: latestVideo.ToEmbed());

							// Save last video id
							streamer.Youtube.LastVideoId = latestVideo.Id;
							streamerUpdated = true;
						}

						// Check live stream
						ExploderAPI.Video liveStream = await ExploderAPI.GetLiveVideo(streamer.Youtube.LinkId);

						// Live stream occurring
						if (liveStream != null && liveStream.Id != streamer.Youtube.LastStreamId)
						{
							await contentCreatorChannel.SendMessageAsync(embed: liveStream.ToLiveEmbed());

							// Save last video id
							streamer.Youtube.LastStreamId = liveStream.Id;
							streamerUpdated = true;
						}
					}

					if (streamerUpdated)
						await ContentCreatorDatabase.Save(streamer);
				}
			}
		}

		private async Task SetContentCreator(CommandMessage message, string identifier, ContentCreator.Type type, string? linkId = null)
		{
			ContentCreator streamer = await ContentCreatorDatabase.LoadOrCreate(message.Author.Id.ToString());

			streamer.DiscordGuildId = message.Guild.Id;
			streamer.DiscordUserId = message.Author.Id;
			streamer.GuildNickName = message.Author.GetName();

			streamer.SetContentInfo(identifier, type, linkId);

			await ContentCreatorDatabase.Save(streamer);

			// Send Embed
			RestUserMessage response = await message.Channel.SendMessageAsync("Added Stream Info", messageReference: message.MessageReference);

			// Delay then delete command and response message
			await Task.Delay(2000);
			await response.DeleteAsync();
			message.DeleteMessage();
		}

		private async Task RemoveContentCreator(CommandMessage message, ContentCreator.Type type)
		{
			ContentCreator? streamer = await ContentCreatorDatabase.Load(message.Author.Id.ToString());
			if (streamer != null)
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

			// Send Embed
			RestUserMessage response = await message.Channel.SendMessageAsync("Removed Creator Info", messageReference: message.MessageReference);

			// Delay then delete command and response message
			await Task.Delay(2000);
			await response.DeleteAsync();
			message.DeleteMessage();
		}
	}
}
