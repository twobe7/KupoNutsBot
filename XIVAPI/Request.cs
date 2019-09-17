// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
	using KupoNuts;
	using Newtonsoft.Json;

	internal static class Request
	{
		private static string key = "4de52eac0f664097957216b24446e29b58219f2d348b482e94f6b53db1fa81d5";

		internal static async Task<T> Send<T>(string route)
		{
			if (!route.StartsWith('/'))
				route = '/' + route;

			if (!route.Contains('?'))
				route += '?';

			string url = "https://xivapi.com" + route + "&private_key=" + key;

			try
			{
				Log.Write("Request: " + url, "XIVAPI");

				WebRequest req = WebRequest.Create(url);
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "XIVAPI");

				return JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, "XIVAPI");
				throw ex;
			}
		}
	}
}
