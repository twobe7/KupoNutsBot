// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Universalis
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public static class HistoryAPI
	{
		public static async Task<GetResponse> Get(string dataCenter, ulong itemId)
		{
			return await Request.Send<GetResponse>("/history/" + dataCenter + "/" + itemId);
		}

		public static async Task<(Entry?, Entry?)> GetBestPrice(string dataCenter, ulong itemId)
		{
			GetResponse response = await Get(dataCenter, itemId);

			ulong? bestHqPrice = ulong.MaxValue;
			Entry? bestHq = null;

			ulong? bestNmPrice = ulong.MaxValue;
			Entry? bestNm = null;

			foreach (Entry entry in response.entries)
			{
				if (entry.pricePerUnit == null)
					continue;

				if (entry.hq == true)
				{
					if (entry.pricePerUnit < bestHqPrice)
					{
						bestHq = entry;
						bestHqPrice = entry.pricePerUnit;
					}
				}
				else
				{
					if (entry.pricePerUnit < bestNmPrice)
					{
						bestNm = entry;
						bestNmPrice = entry.pricePerUnit;
					}
				}
			}

			return (bestHq, bestNm);
		}

#pragma warning disable SA1307
		[Serializable]
		public class GetResponse
		{
			public string? dcName;
			public ulong? itemID;
			public ulong? lastUploadTime;
			public List<Entry> entries = new List<Entry>();
		}

		[Serializable]
		public class Entry
		{
			public bool? hq;
			public ulong? pricePerUnit;
			public ulong? timestamp;
			public string? worldName;
		}
	}
}
