// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Tenor
{
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using FC;
	using FC.API;
	using FC.Serialization;

	internal static class Request
	{
		private static string? key;

		private static string? Key
		{
			get
			{
				if (string.IsNullOrEmpty(key))
					key = Settings.Load().TenorKey;

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

			string url = $"https://g.tenor.com/v1{route}&key={Key}";

			try
			{
				Log.Write($"Request: {url}", "Tenor");

				using HttpClient client = new();
				using var stream = await client.GetStreamAsync(url);
				using StreamReader reader = new(stream);

				string json = await reader.ReadToEndAsync();

				Log.Write($"Response: {json.Length} characters", "Tenor");

				return Serializer.DeserializeResponse<T>(json);
			}
			catch (WebException webEx)
			{
				HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
				if (errorResponse.StatusCode == HttpStatusCode.NotFound)
				{
					return Activator.CreateInstance<T>();
				}

				throw;
			}
			catch (Exception ex)
			{
				Log.Write($"Error: {ex.Message}", "Tenor");
				throw;
			}
		}
	}
}
