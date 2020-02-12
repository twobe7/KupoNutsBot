// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Lodestone
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

			string url = "http://na.lodestonenews.com" + route;

			try
			{
				Log.Write("Request: " + url, "Lodestone");

				WebRequest req = WebRequest.Create(url);
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "Lodestone");

				return JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, "Lodestone");
				throw ex;
			}
		}
	}
}
