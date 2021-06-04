// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace MusicPlayer
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using CliWrap;
	using Discord;
	using Discord.Audio;
	using Discord.WebSocket;
	using YoutubeExplode;
    using YoutubeExplode.Common;

    public class MusicPlayer
	{
		public class AudioClient
		{
			public IAudioClient client { get; set; }
			public AudioOutStream? currentStream { get; set; }
			public DateTime? streamInactiveAt { get; set; }
		}

		private readonly ConcurrentDictionary<ulong, AudioClient> connectedChannels = new ConcurrentDictionary<ulong, AudioClient>();

		public async Task JoinAudio(IGuild guild, IVoiceChannel target)
		{
			if (this.connectedChannels.TryGetValue(guild.Id, out _))
				return;

			if (target.Guild.Id != guild.Id)
				return;

			AudioClient audioClient = new AudioClient();
			audioClient.client = await target.ConnectAsync();

			if (this.connectedChannels.TryAdd(guild.Id, audioClient))
			{
				FC.Log.Write($"Connected to voice on {guild.Name}.", "Bot - Audio");

				// Leave audio if no music
				_ = Task.Run(async () => await IdleStreamStop());
			}
		}

		public async Task LeaveAudio(IGuild guild)
		{
			if (this.connectedChannels.TryRemove(guild.Id, out AudioClient audioClient))
			{
				await audioClient.client.StopAsync();
				FC.Log.Write($"Disconnected from voice on {guild.Name}.", "Bot - Audio");
			}
		}

		public async Task LeaveAudio(ulong guildId)
		{
			if (this.connectedChannels.TryRemove(guildId, out AudioClient audioClient))
				await audioClient.client.StopAsync();
		}

		public async Task Disconnect()
		{
			foreach (KeyValuePair<ulong, AudioClient> connected in connectedChannels)
			{
				await connected.Value.client.StopAsync();
				FC.Log.Write($"Disconnected from voice on {connected.Key}.", "Bot - Audio");
			}
			
		}

		public async Task SendYoutubeAudioAsync(IMessage message, IGuild guild, IMessageChannel channel, string query)
		{
			// Check that we have a query
			if (string.IsNullOrWhiteSpace(query))
				return;

			// Init yt client
			YoutubeClient youtube = new YoutubeClient();
			Stream ytStream;
			string id = string.Empty;

			// Try to parse the url given to return just the video id
			if (query.Contains("v=") && query.Length >= 3)
			{
				
				if (query.Contains("&"))
				{
					id = Regex.Match(query, "v=(.*?)&").ToString();
					id = id.Substring(2, id.Length - 3);
				}
				else
				{
					id = Regex.Match(query, "v=(.*?)$").ToString();
					id = id.Substring(2, id.Length - 2);
				}
			}
			else
			{
				// Get the stream from YT via search
				IReadOnlyList<YoutubeExplode.Search.VideoSearchResult> searchResult = await youtube.Search.GetVideosAsync(query).CollectAsync(1);
				id = searchResult.Getfirst().Id;
			}

			// Get the stream from YT via video id
			YoutubeExplode.Videos.Streams.StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(id);
			YoutubeExplode.Videos.Streams.AudioOnlyStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().Getfirst();
			ytStream = await youtube.Videos.Streams.GetAsync(streamInfo);

			// Convert yt stream
			MemoryStream memoryStream = new MemoryStream();
			await Cli.Wrap("ffmpeg")
				.WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
				.WithStandardInputPipe(PipeSource.FromStream(ytStream))
				.WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
				.ExecuteAsync();

			if (this.connectedChannels.TryGetValue(guild.Id, out AudioClient audioClient))
			{

				// Clear stream before beginning
				if (audioClient.currentStream != null)
				{
					audioClient.currentStream.Dispose();
				}

				AudioOutStream discord = audioClient.client.CreatePCMStream(AudioApplication.Mixed);
				audioClient.currentStream = discord;

				try
				{
					// Delete calling command
					await message.DeleteAsync();

					// Start playing music
					await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length);
				}
				finally
				{
					await discord.FlushAsync();
					await discord.DisposeAsync();

					// Clear current stream
					audioClient.currentStream = null;
					audioClient.streamInactiveAt = DateTime.Now;
				}
			}
		}

		private async Task<Task> IdleStreamStop()
		{
			while (this.connectedChannels.Count > 0)
			{
				await Task.Delay(30000);

				FC.Log.Write("Checking For Inactive Audio Clients from total of " + this.connectedChannels.Count, "Bot - Audio");

				DateTime runTime = DateTime.Now;
				foreach (KeyValuePair<ulong, AudioClient> connectedChannel in this.connectedChannels)
				{
					AudioClient client = connectedChannel.Value;
					if (client.currentStream == null && (!client.streamInactiveAt.HasValue || (runTime - client.streamInactiveAt.Value).TotalSeconds > 30))
						await this.LeaveAudio(connectedChannel.Key);
				}
			}

			FC.Log.Write("All Audio Streams Closed", "Bot - Audio");
			return Task.CompletedTask;
		}
	}
}
