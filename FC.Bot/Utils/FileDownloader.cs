// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Utils
{
	using System;
	using System.IO;
	using System.Net.Http;
	using System.Threading.Tasks;

	public static class FileDownloader
	{
		public static async Task<Task> Download(string url, string path)
		{
			string? dir = Path.GetDirectoryName(path);

			if (dir is null)
				throw new Exception("Failed to get director at path: \"" + path + "\"");

			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			Log.Write("download: " + url + " to " + path, "Bot");

			using HttpClient client = new ();
			using var s = await client.GetStreamAsync(url);
			using var fs = new FileStream(path, FileMode.CreateNew);
			await s.CopyToAsync(fs);

			return Task.CompletedTask;
		}
	}
}
