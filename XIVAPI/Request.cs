// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
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

				using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
				var stream = await client.GetStreamAsync(url);
				StreamReader reader = new(stream);
				string json = await reader.ReadToEndAsync();

				Log.Write($"Response: {json.Length} characters", "XIVAPI");

				// dirty hack to handle GameContentLinks being sometimes an array, and sometimes an object...
				json = json.Replace("\"GameContentLinks\":[]", "\"GameContentLinks\":null");

				T result = Serializer.Deserialize<T>(json)
					?? throw new InvalidDataException("Unable to deserialize");

				result.Json = json;
				return result;
			}
			catch (WebException webEx)
			{
				HttpWebResponse? errorResponse = (HttpWebResponse?)webEx.Response;
				if (errorResponse?.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<T>();
				}

				throw;
			}
			catch (Exception ex)
			{
				Log.Write($"Error: {ex.Message}", "XIVAPI");
				throw;
			}
		}
	}
}
