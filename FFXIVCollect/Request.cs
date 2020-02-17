// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FFXIVCollect
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
	using FC;
	using Newtonsoft.Json;

	internal static class Request
	{
		internal static async Task<T> Send<T>(string route)
		{
			if (!route.StartsWith('/'))
				route = '/' + route;

			string url = "https://ffxivcollect.com/api/" + route;

			try
			{
				Log.Write("Request: " + url, "FFXIVCollect");

				WebRequest req = WebRequest.Create(url);
				req.Timeout = 5000;
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "FFXIVCollect");

				return JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, "FFXIVCollect");
				throw ex;
			}
		}
	}
}
