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
	using FC.Serialization;

	internal static class Request
	{
		internal static async Task<T> Send<T>(string route)
		{
			if (!route.StartsWith('/'))
				route = '/' + route;

			string url = "https://lodestonenews.com" + route;

			try
			{
				Log.Write("Request: " + url, "Lodestone");

				WebRequest req = WebRequest.Create(url);
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Log.Write("Response: " + json.Length + " characters", "Lodestone");

				return Serializer.Deserialize<T>(json);
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, "Lodestone");
				throw ex;
			}
		}

		internal static async Task<string> Detail(string url)
		{
			try
			{
				Log.Write("Detail: " + url, "Lodestone");

				WebRequest req = WebRequest.Create(url);
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string html = await reader.ReadToEndAsync();

				Log.Write("Response: " + html.Length + " characters", "Lodestone");

				int startIndex = html.IndexOf("news__detail__wrapper") + 23;
				int endIndex = html.IndexOf("</div>", startIndex);
				string detail = html.Substring(startIndex, endIndex - startIndex);

				detail = detail.Replace("<br>", string.Empty);

				return detail;
			}
			catch (Exception ex)
			{
				Log.Write("Error: " + ex.Message, "Lodestone");
				throw ex;
			}
		}
	}
}
