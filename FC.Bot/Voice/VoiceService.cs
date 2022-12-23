// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System.Collections.Concurrent;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Audio;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using MusicPlayer;

	public class VoiceService : ServiceBase
	{
		private readonly ConcurrentDictionary<ulong, IAudioClient> connectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
		private MusicPlayer? musicPlayer;

		public VoiceService(DiscordSocketClient discordClient)
		{
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			this.musicPlayer = new MusicPlayer();
		}

		public override Task Shutdown()
		{
			_ = this.musicPlayer?.Disconnect();

			return base.Shutdown();
		}

		// You *MUST* mark these commands with 'RunMode.Async'
		// otherwise the bot will not respond until the Task times out.
		[Command("Join", Permissions.Everyone, "Join audio to channel", CommandCategory.Music, showWait: false)]
		public async Task<bool> JoinCmd(CommandMessage message)
		{
			if (this.musicPlayer != null)
			{
				IVoiceState voiceState = message.Author as IVoiceState;
				if (voiceState.VoiceChannel != null)
				{
					return await this.musicPlayer.JoinAudio(message.Guild, voiceState.VoiceChannel);
				}
				else
				{
					Discord.Rest.RestUserMessage responseMessage = await message.Channel.SendMessageAsync("You need to join a voice channel first, _kupo!_", messageReference: message.MessageReference);

					await Task.Delay(3000);

					// Delete response command
					await responseMessage.DeleteAsync();
				}
			}

			// Delete calling command
			message.DeleteMessage();

			return false;
		}

		[Command("Leave", Permissions.Everyone, "Disconnect audio from channel", CommandCategory.Music)]
		public async Task LeaveCmd(CommandMessage message)
		{
			if (this.musicPlayer == null)
				return;

			await this.musicPlayer.LeaveAudio(message.Guild);

			// Delete calling command
			message.DeleteMessage();
		}

		[Command("Play", Permissions.Everyone, "Play a song", CommandCategory.Music, showWait: false)]
		public async Task PlayCmd(CommandMessage message, string song)
		{
			if (this.musicPlayer == null)
				return;

			bool canPlay = true;

			// Check if connected to Voice channel and try to connect if not
			if (!this.musicPlayer.IsConnected(message.Guild.Id))
				canPlay = await this.JoinCmd(message);

			// If connected, play song
			if (canPlay)
				await this.musicPlayer.SendYoutubeAudioAsync(message.Message, message.Author, song);
		}

		[Command("Skip", Permissions.Everyone, "Skip the current song", CommandCategory.Music, showWait: false)]
		public void SkipCmd(CommandMessage message)
		{
			if (this.musicPlayer == null)
				return;

			this.musicPlayer.SkipCurrentSong(message.Message, message.Guild.Id);
		}

		[Command("Playlist", Permissions.Everyone, "Show the upcoming songs", CommandCategory.Music, showWait: false)]
		public async void ShowPlaylist(CommandMessage message)
		{
			if (this.musicPlayer == null)
				return;

			await this.musicPlayer.ShowPlaylistEmbed(message.Message, message.Guild.Id);
		}
	}
}
