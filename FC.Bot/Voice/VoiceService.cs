// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System.Collections.Concurrent;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Audio;
	using FC.Bot.Commands;
	using MusicPlayer;

	public class VoiceService : ServiceBase
	{
		private readonly ConcurrentDictionary<ulong, IAudioClient> connectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
		private MusicPlayer? musicPlayer;

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
		[Command("Join", Permissions.Everyone, "Join audio to channel")]
		public async Task JoinCmd(CommandMessage message)
		{
			if (this.musicPlayer == null)
				return;

			await this.musicPlayer.JoinAudio(message.Guild, (message.Author as IVoiceState).VoiceChannel);
		}

		[Command("Leave", Permissions.Everyone, "Disconnect audio from channel")]
		public async Task LeaveCmd(CommandMessage message)
		{
			if (this.musicPlayer == null)
				return;

			await this.musicPlayer.LeaveAudio(message.Guild);
		}

		[Command("Play", Permissions.Everyone, "Play a song", showWait: false)]
		public async Task PlayCmd(CommandMessage message, string song)
		{
			if (this.musicPlayer == null)
				return;

			await this.musicPlayer.SendYoutubeAudioAsync(message.Message, message.Guild, message.Channel, song);
		}
	}
}
