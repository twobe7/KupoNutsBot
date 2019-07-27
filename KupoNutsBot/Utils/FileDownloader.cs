// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Utils
{
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;

	public static class FileDownloader
	{
		public static Task Download(string url, string path)
		{
			string dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (WebClient client = new WebClient())
			{
				Log.Write("download: " + url + " to " + path);
				client.DownloadFile(url, path);
			}

			return Task.CompletedTask;
		}
	}
}
