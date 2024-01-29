// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FFXIVCollect;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FC.API;

public static class SearchAPI
{
	public enum SearchType
	{
		Characters = 0,
		Mounts = 1,
	}

	public static async Task<List<Result>> Search(SearchType type, string name)
	{
		string route = string.Format("/{0}?name_en_cont={1}", type.ToString().ToLower(), name);

		SearchResponse response = await Request.Send<SearchResponse>(route);

		if (response.Results == null)
			return new List<Result>();

		return response.Results;
	}

	[Serializable]
	public class SearchResponse
	{
		/* public Pagination? Pagination { get; set; } */
		public List<Result>? Results { get; set; }
	}
}