// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace MusicPlayer
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using CliWrap;
	using Discord;
	using Discord.Audio;
	using YoutubeExplode;
	using YoutubeExplode.Common;
	using YoutubeExplode.Videos.Streams;

	public class MusicPlayer
	{
		private YoutubeClient youtube = new YoutubeClient();

		public class VideoInfo
		{
			public VideoInfo(string requestor)
			{
				this.RequestedBy = requestor;
			}

			public string Title { get; set; }
			public string Duration { get; set; }
			public AudioOnlyStreamInfo Stream { get; set; }
			public string Thumbnail { get; set; }
			public string RequestedBy { get; set; }

			public EmbedFieldBuilder GetCurrentSongEmbed()
			{
				return new EmbedFieldBuilder().WithName(this.Title).WithValue(this.GetPlayInfo);
			}

			public string GetPlayInfo => $"Requested By: *{this.RequestedBy}*. \n Duration: *{this.Duration}*";
		}
		public class AudioClient
		{
			public IAudioClient Client { get; set; }
			public AudioOutStream CurrentStream { get; set; }
			public DateTime? StreamInactiveAt { get; set; }
			public VideoInfo CurrentSong { get; set; }
			public List<VideoInfo> PlayList { get; set; } = new List<VideoInfo>();

			/// <summary>
			/// Sets current song and removes from playlist
			/// </summary>
			/// <returns>Stream of next song in playlist or null if none in Playlist</returns>
			public AudioOnlyStreamInfo GetNextSong()
			{
				if (this.PlayList.Count > 0)
				{
					this.CurrentSong = this.PlayList[0];
					this.PlayList.RemoveAt(0);
				}
				else
				{
					if (this.CurrentStream != null)
						this.CurrentStream.Dispose();

					this.CurrentStream = null;
					this.CurrentSong = null;
					this.StreamInactiveAt = DateTime.Now;
				}

				return this.CurrentSong?.Stream;
			}
		}

		private readonly ConcurrentDictionary<ulong, AudioClient> connectedChannels = new ConcurrentDictionary<ulong, AudioClient>();

		public bool IsConnected(ulong guildId)
		{
			return this.connectedChannels.ContainsKey(guildId);
		}

		public async Task<bool> JoinAudio(IGuild guild, IVoiceChannel target)
		{
			if (this.IsConnected(guild.Id))
				return false;

			if (target.Guild.Id != guild.Id)
				return false;

			AudioClient audioClient = new AudioClient();
			audioClient.Client = await target.ConnectAsync();

			if (this.connectedChannels.TryAdd(guild.Id, audioClient))
			{
				FC.Log.Write($"Connected to voice on {guild.Name}.", "Bot - Audio");

				// Leave audio if no music
				_ = Task.Run(async () => await IdleStreamStop());

				return true;
			}

			return false;
		}

		public async Task LeaveAudio(IGuild guild)
		{
			if (this.connectedChannels.TryRemove(guild.Id, out AudioClient audioClient))
			{
				await audioClient.Client.StopAsync();
				FC.Log.Write($"Disconnected from voice on {guild.Name}.", "Bot - Audio");
			}
		}

		public async Task LeaveAudio(ulong guildId)
		{
			if (this.connectedChannels.TryRemove(guildId, out AudioClient audioClient))
				await audioClient.Client.StopAsync();
		}

		public async Task Disconnect()
		{
			foreach (KeyValuePair<ulong, AudioClient> connected in connectedChannels)
			{
				await connected.Value.Client.StopAsync();
				FC.Log.Write($"Disconnected from voice on {connected.Key}.", "Bot - Audio");
			}
		}

		public async Task SendYoutubeAudioAsync(IMessage message, IGuildUser user, string query)
		{
			// Check that we have a query
			if (string.IsNullOrWhiteSpace(query))
				return;

			if (this.connectedChannels.TryGetValue(user.GuildId, out AudioClient audioClient))
			{
				VideoInfo videoInfo = new VideoInfo(user.GetName());

				// Try to parse the url given to return just the video id
				string id;
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

					YoutubeExplode.Videos.Video video = await youtube.Videos.GetAsync(id);
					videoInfo.Title = video.Title;
					videoInfo.Duration = video.Duration.ToString();
					videoInfo.Thumbnail = video.Thumbnails.Getfirst().Url;
				}
				else
				{
					// Get the stream from YT via search
					IReadOnlyList<YoutubeExplode.Search.VideoSearchResult> searchResult = await youtube.Search.GetVideosAsync(query).CollectAsync(1);
					YoutubeExplode.Search.VideoSearchResult result = searchResult.Getfirst();

					videoInfo.Title = result.Title;
					videoInfo.Duration = result.Duration.ToString();
					videoInfo.Thumbnail = result.Thumbnails.Getfirst().Url;

					id = result.Id;
				}

				// Get the stream from YT via video id
				StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(id);
				AudioOnlyStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().Getfirst();

				videoInfo.Stream = streamInfo;

				// If song currently playing, add to playlist
				if (audioClient.CurrentStream != null)
				{
					audioClient.PlayList.Add(videoInfo);

					// Delete calling command
					await message.DeleteAsync();
				}
				else
				{
					// Play song
					audioClient.CurrentSong = videoInfo;
					this.StartStream(youtube, audioClient, streamInfo, message);
				}
			}
		}

		public void SkipCurrentSong(IMessage message, ulong guildId)
		{
			if (this.connectedChannels.TryGetValue(guildId, out AudioClient audioClient))
			{
				AudioOnlyStreamInfo nextSong = audioClient.GetNextSong();
				if (nextSong != null)
				{
					this.StartStream(youtube, audioClient, nextSong, message);
				}
				else
				{
					message.DeleteAsync();
				}
			}
		}

		public async Task ShowPlaylistEmbed(IMessage message, ulong guildId)
		{
			if (this.connectedChannels.TryGetValue(guildId, out AudioClient audioClient))
			{
				EmbedBuilder embed = new EmbedBuilder()
					.WithTitle("Kupo Nuts FM - Upcoming Tracks");

				if (audioClient.CurrentSong != null)
				{
					embed.Description += "**Currently playing:**";
					embed.AddField(audioClient.CurrentSong.GetCurrentSongEmbed());
				}

				if (audioClient.PlayList.Count > 0)
				{
					string upNextDescription = string.Empty;
					foreach (VideoInfo video in audioClient.PlayList.Take(5))
					{
						upNextDescription += "• " + video.Title + Environment.NewLine;
					}

					embed.AddField(new EmbedFieldBuilder()
					{
						Name = "Up Next",
						Value = upNextDescription,
					});

					embed.WithFooter(new EmbedFooterBuilder { Text = "Total songs in queue: " + audioClient.PlayList.Count });
				}
				else if (audioClient.CurrentSong == null)
				{
					embed.Description = "No queued songs, add one with the play command!";
				}

				// Post playlist embed
				await message.Channel.SendMessageAsync(embed: embed.Build());
			}
			else
			{
				IUserMessage responseMessage = await message.Channel.SendMessageAsync("No connected players, _kupo!_");

				await Task.Delay(3000);

				// Delete response message
				await responseMessage.DeleteAsync();
			}

			// Delete calling command
			await message.DeleteAsync();

			return;
		}

		private async void StartStream(YoutubeClient youtube, AudioClient audioClient, AudioOnlyStreamInfo streamInfo, IMessage? message)
		{
			Stream ytStream = await youtube.Videos.Streams.GetAsync(streamInfo);

			// Convert yt stream
			MemoryStream memoryStream = new MemoryStream();
			await Cli.Wrap("ffmpeg")
				.WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
				.WithStandardInputPipe(PipeSource.FromStream(ytStream))
				.WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
				.ExecuteAsync();

			// Clear stream before beginning
			if (audioClient.CurrentStream != null)
			{
				audioClient.CurrentStream.Dispose();
				audioClient.CurrentStream = null;
			}
			
			AudioOutStream discord = audioClient.Client.CreatePCMStream(AudioApplication.Mixed);
			audioClient.CurrentStream = discord;

			// Delete calling command
			if (message != null)
				await message.DeleteAsync();

			// Start playing music
			await this.WriteStreamToVoiceChannel(audioClient, discord, memoryStream);
		}

		private async Task WriteStreamToVoiceChannel(AudioClient audioClient, AudioOutStream discord, MemoryStream memoryStream)
		{
			try
			{
				await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length);
			}
			finally
			{
				await memoryStream.DisposeAsync();

				await discord.FlushAsync();
				await discord.DisposeAsync();

				// Clear current stream
				audioClient.CurrentStream = null;

				// Play next
				AudioOnlyStreamInfo nextSong = audioClient.GetNextSong();
				if (nextSong != null)
					this.StartStream(youtube, audioClient, nextSong, null);
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
					if (client.CurrentStream == null && (!client.StreamInactiveAt.HasValue || (runTime - client.StreamInactiveAt.Value).TotalSeconds > 30))
						await this.LeaveAudio(connectedChannel.Key);
				}
			}

			FC.Log.Write("All Audio Streams Closed", "Bot - Audio");
			return Task.CompletedTask;
		}
	}
}
