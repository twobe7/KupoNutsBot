// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Streamers
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

	public class StreamerService : ServiceBase
	{
		public static Table<Streamer> StreamerDatabase = new Table<Streamer>("KupoNuts_Streamers", 0);

		public override async Task Initialize()
		{
			await base.Initialize();

			await StreamerDatabase.Connect();

			ScheduleService.RunOnSchedule(this.Update, 10);
			await this.Update();
		}

		public override Task Shutdown()
		{
			return base.Shutdown();
		}

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

		[Command("IStreamTwitch", Permissions.Everyone, "Set your twitch stream")]
		public async Task SetTwitchStream(CommandMessage message, string username)
		{
			Streamer streamer = await StreamerDatabase.LoadOrCreate(message.Author.Id.ToString());

			streamer.DiscordGuildId = message.Guild.Id;
			streamer.DiscordUserId = message.Author.Id;
			streamer.GuildNickName = message.Author.GetName();
			streamer.Twitch = new Streamer.ContentInfo(username);

			await StreamerDatabase.Save(streamer);

			// Send Embed
			RestUserMessage response = await message.Channel.SendMessageAsync("Added Stream Info, _kupo!_", messageReference: message.MessageReference);

			// Delay then delete command and response message
			await Task.Delay(2000);
			await response.DeleteAsync();
			message.DeleteMessage();
		}

		[Command("RemoveStreamTwitch", Permissions.Everyone, "Remove your set Twitch stream")]
		public async Task RemoveTwitchStream(CommandMessage message)
		{
			Streamer? streamer = await StreamerDatabase.Load(message.Author.Id.ToString());
			if (streamer != null)
			{
				if (streamer.Youtube == null)
				{
					// Doesn't stream YT - remove from db completely
					await StreamerDatabase.Delete(streamer);
				}
				else
				{
					// Remove Twitch stream and save
					streamer.Twitch = null;
					await StreamerDatabase.Save(streamer);
				}
			}

			// Send Embed
			RestUserMessage response = await message.Channel.SendMessageAsync("Removed Stream Info, _kupo!_", messageReference: message.MessageReference);

			// Delay then delete command and response message
			await Task.Delay(2000);
			await response.DeleteAsync();
			message.DeleteMessage();
		}

		[Command("WhoStreams", Permissions.Everyone, "View current streamers")]
		public async Task ViewStreamers(CommandMessage message)
		{
			// Load streamers
			List<Streamer> streamers = await StreamerDatabase.LoadAll(new Dictionary<string, object> { { "DiscordGuildId", message.Guild.Id } });

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("Streamers");

			// Add thumbnail
			embed.AddThumbnail(message.Guild.IconUrl);

			if (streamers == null || streamers.Count == 0)
			{
				// TODO: Add YT when implemented
				embed.Description = "No streamers found!\nUsers can add themselves with IStreamTwitch command";
			}
			else
			{
				uint idx = 1;
				StringBuilder desc = new StringBuilder();

				foreach (Streamer streamer in streamers)
				{
					desc.Append($"{idx++}. {streamer.GuildNickName} | ");

					if (streamer.Twitch != null)
					{
						desc.Append($"Twitch: {streamer.Twitch.UserName}");
					}

					if (streamer.Youtube != null)
					{
						if (streamer.Twitch != null)
							desc.Append(" | ");

						desc.Append($"Youtube: {streamer.Youtube.UserName}");
					}
				}

				embed.Description = desc.ToString();
			}

			// Send Embed
			await message.Channel.SendMessageAsync(embed: embed.Build(), messageReference: message.MessageReference);
		}

		////[Command("Events", Permissions.Administrators, "Checks event notifications")]
		public async Task Update()
		{
			// Load streamers
			List<Streamer> streamers = await StreamerDatabase.LoadAll();

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
				foreach (Streamer streamer in streamers.Where(x => x.DiscordGuildId == guild.Id))
				{
					// Twitch streamer
					if (streamer.Twitch != null && !string.IsNullOrWhiteSpace(streamer.Twitch.UserName))
					{
						StreamerAPI.Stream stream = await StreamerAPI.GetStreams(streamer.Twitch.UserName);

						// Streamer is live and current stream hasn't been posted
						if (stream.IsLive && stream.Id != streamer.Twitch.LastStreamId)
						{
							await contentCreatorChannel.SendMessageAsync(embed: stream.ToEmbed());

							// Save streamer id
							streamer.Twitch.LastStreamId = stream.Id;
							await StreamerDatabase.Save(streamer);
						}
					}
				}
			}
		}
	}
}
