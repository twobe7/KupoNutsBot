// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using System.Threading.Tasks;
	using Discord.Audio;
	using Discord.WebSocket;

	public static class MusicPlayer
	{
		private static async Task PlayMusic(ulong channelId, DiscordSocketClient client)
		{
			try
			{
				Console.WriteLine("Playing music");

				SocketChannel channel = client.GetChannel(channelId);
				if (channel is SocketVoiceChannel voiceChannel)
				{
					IAudioClient audioClient = await voiceChannel.ConnectAsync();

					// Create FFmpeg using the previous example
					using (Process ffmpeg = CreateStream("test.mp3"))
					using (System.IO.Stream output = ffmpeg.StandardOutput.BaseStream)
					using (AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music, null, 2000))
					{
						try
						{
							await output.CopyToAsync(discord);
						}
						finally
						{
							await discord.FlushAsync();
						}
					}

					/*using (MP3Stream mp3Stream = new MP3Stream("test.mp3", 4096))
					using (AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
					{
						try
						{
							await mp3Stream.CopyToAsync(discord);
						}
						finally
						{
							await discord.FlushAsync();
						}
					}*/
				}
				else
				{
					throw new Exception("Channel was not voice channel");
				}

				Console.WriteLine("music done");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during music: " + ex.Message);
			}
		}

		private static Process CreateStream(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = "ffmpeg.exe",
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}
	}
}
