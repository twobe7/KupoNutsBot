// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace PaissaHouse
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
			where T : ResponseBase
		{
			if (!route.StartsWith('/'))
				route = '/' + route;

			string url = $"https://paissadb.zhu.codes{route}";

			try
			{
				Log.Write($"Request: {url}", "Paissa House");

				using HttpClient client = new();
				using var stream = await client.GetStreamAsync(url);
				using StreamReader reader = new(stream);

				string json = await reader.ReadToEndAsync();

				Log.Write($"Response: {json.Length} characters", "Paissa House");

				// Data is returned in snake case format
				T result = Serializer.DeserializeResponse<T>(json, Serializer.SnakeCaseOptions);

				try
				{
					DisabledResponse disabledResponse = Serializer.Deserialize<DisabledResponse>(json, Serializer.SnakeCaseOptions);
					if (disabledResponse != null && result != null)
						result.ErrorMessage = disabledResponse.Message;
				}
				finally
				{ }

				return result;
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
				Log.Write($"Error: {ex.Message}", "Paissa House");
				throw;
			}
		}

		internal class DisabledResponse
		{
			public string Message;
			public bool Indefinite;
		}
	}
}
