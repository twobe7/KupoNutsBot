// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace PaissaHouse
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
	using FC;
	using FC.Serialization;

	internal static class Request
	{
		////private static string? key;

		////private static string? Key
		////{
		////	get
		////	{
		////		if (string.IsNullOrEmpty(key))
		////			key = Settings.Load().TenorKey;

		////		return key;
		////	}
		////}

		internal static async Task<T> Send<T>(string route)
			where T : ResponseBase
		{
			if (!route.StartsWith('/'))
				route = '/' + route;

			string url = "https://paissadb.zhu.codes" + route;

			try
			{
				Log.Write("Request: " + url, "Paissa House");

				WebRequest req = WebRequest.Create(url);
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "Paissa House");

				T result = NSSerializer.Deserialize<T>(json);
				result.Json = json;

				try
				{
					DisabledResponse disabledResponse = NSSerializer.Deserialize<DisabledResponse>(json);
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

				throw webEx;
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, "Paissa House");
				throw ex;
			}
		}

		internal class DisabledResponse
		{
			public string Message;
			public bool Indefinite;
		}
	}
}
