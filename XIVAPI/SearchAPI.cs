﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FC.API;

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
		List<Result> results = new ();

		string route = "/search?string=" + name;

		// XIVAPI returns all actions, including non active actions
		// Best guess is anything with ClassJobLevel = 0 is inactive so filter these out
		if (type.ToLower() == "action")
		{
			route += "&filters=ClassJobLevel>0";
		}

		SearchResponse response = await Request.Send<SearchResponse>(route);

		if (response?.Results != null)
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
		public Pagination? Pagination { get; set; }
		public List<Result>? Results { get; set; }
	}
}
