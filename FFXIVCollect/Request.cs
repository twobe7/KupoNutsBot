// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FFXIVCollect
{
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using FC;
	using FC.Serialization;

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

				using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
				StreamReader reader = new (await client.GetStreamAsync(url));
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "FFXIVCollect");

				return Serializer.Deserialize<T>(json)
					?? throw new InvalidDataException("Unable to deserialize");
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, "FFXIVCollect");
				throw;
			}
		}
	}
}
