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
		private static string? key;

		private static string? Key
		{
			get
			{
				if (string.IsNullOrEmpty(key))
					key = Settings.Load().XIVAPIKey;

				return key;
			}
		}

		internal static async Task<T> Send<T>(string route)
			where T : ResponseBase
		{
			if (!route.StartsWith('/'))
				route = '/' + route;

			if (!route.Contains('?'))
				route += '?';

			string url = "https://xivapi.com" + route + "&private_key=" + Key;

			try
			{
				Log.Write("Request: " + url, "XIVAPI");

				WebRequest req = WebRequest.Create(url);
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "XIVAPI");

				// dirty hack to handle GameContentLinks being sometimes an array, and sometimes an object...
				json = json.Replace("\"GameContentLinks\":[]", "\"GameContentLinks\":null");

				T result = JsonConvert.DeserializeObject<T>(json);
				result.Json = json;
				return result;
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<T>();
				}

				throw webEx;
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, "XIVAPI");
				throw ex;
			}
		}
	}
}
