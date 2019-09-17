// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Utils
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;

	public static class FileDownloader
	{
		public static Task Download(string url, string path)
		{
			string? dir = Path.GetDirectoryName(path);

			if (dir is null)
				throw new Exception("Failed to get director at path: \"" + path + "\"");

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (WebClient client = new WebClient())
			{
				Log.Write("download: " + url + " to " + path, "Bot");
				client.DownloadFile(url, path);
			}

			return Task.CompletedTask;
		}
	}
}
