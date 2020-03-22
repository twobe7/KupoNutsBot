// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Universalis
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
	using FC;
	using FC.Serialization;

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

				return Serializer.Deserialize<T>(json);
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, @"Universalis");
				throw ex;
			}
		}
	}
}
