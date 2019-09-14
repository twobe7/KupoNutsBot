// This document is intended for use by Kupo Nut Brigade developers.

namespace Lodestone
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
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
				Console.WriteLine("[Lodestone] Request: " + url);

				WebRequest req = WebRequest.Create(url);
				WebResponse response = await req.GetResponseAsync();
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = await reader.ReadToEndAsync();

				Console.WriteLine("[Lodestone] Response: " + json.Length + " characters");

				return JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				Console.WriteLine("[Lodestone] Error: " + ex.Message);
				throw ex;
			}
		}
	}
}
