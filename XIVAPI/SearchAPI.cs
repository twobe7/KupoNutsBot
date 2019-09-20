// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public static class SearchAPI
	{
		public static async Task<List<Result>> Search(string name)
		{
			string route = "/search?string=" + name;

			SearchResponse response = await Request.Send<SearchResponse>(route);

			if (response.Results == null)
				return new List<Result>();

			return response.Results;
		}

		public static async Task<List<Result>> Search(string name, string type)
		{
			List<Result> results = new List<Result>();

			string route = "/search?string=" + name;

			SearchResponse response = await Request.Send<SearchResponse>(route);

			if (response.Results != null)
			{
				foreach (Result result in response.Results)
				{
					if (result.UrlType != type)
						continue;

					results.Add(result);
				}
			}

			return results;
		}

		[Serializable]
		public class SearchResponse : ResponseBase
		{
			public Pagination? Pagination;
			public List<Result>? Results;
		}

		[Serializable]
		public class Result
		{
			public ulong? ID;
			public string? Icon;
			public string? Name;
			public string? Url;
			public string? UrlType;
		}
	}
}
