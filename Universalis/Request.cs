// This document is intended for use by Kupo Nut Brigade developers.

namespace Universalis
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
	using KupoNuts;
	using Newtonsoft.Json;

	internal static class Request
	{
		internal static async Task<T> Send<T>(string route)
		{
			if (!route.StartsWith('/'))
				route = '/' + route;

			string url = "https://universalis.app/api/" + route;

			try
			{
				Log.Write("Request: " + url, @"Universalis");

				WebRequest req = WebRequest.Create(url);
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", @"Universalis");

				return JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, @"Universalis");
				throw ex;
			}
		}
	}
}
