// This document is intended for use by Kupo Nut Brigade developers.

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

		public static async Task<Entry?> GetBestPrice(string dataCenter, ulong itemId)
		{
			GetResponse response = await Get(dataCenter, itemId);

			ulong? bestPrice = ulong.MaxValue;
			Entry? best = null;
			foreach (Entry entry in response.entries)
			{
				if (entry.pricePerUnit == null)
					continue;

				if (entry.pricePerUnit < bestPrice)
				{
					best = entry;
					bestPrice = entry.pricePerUnit;
				}
			}

			return best;
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
