// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Universalis
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public static class MarketAPI
	{
		public static async Task<GetResponse> Get(string dataCenter, ulong itemId)
		{
			return await Request.Send<GetResponse>("/" + dataCenter + "/" + itemId);
		}

		public static async Task<(History?, History?)> GetBestPrice(string dataCenter, ulong itemId)
		{
			GetResponse response = await Get(dataCenter, itemId);

			ulong? bestHqPrice = ulong.MaxValue;
			History? bestHq = null;

			ulong? bestNmPrice = ulong.MaxValue;
			History? bestNm = null;

			foreach (History entry in response.RecentHistory)
			{
				if (entry.PricePerUnit == null)
					continue;

				if (entry.Hq == true)
				{
					if (entry.PricePerUnit < bestHqPrice)
					{
						bestHq = entry;
						bestHqPrice = entry.PricePerUnit;
					}
				}
				else
				{
					if (entry.PricePerUnit < bestNmPrice)
					{
						bestNm = entry;
						bestNmPrice = entry.PricePerUnit;
					}
				}
			}

			return (bestHq, bestNm);
		}

		[Serializable]
		public class GetResponse
		{
			public string? DcName { get; set; }
			public ulong? ItemID { get; set; }
			public ulong? LastUploadTime { get; set; }
			public List<Listing> Listings { get; set; } = new List<Listing>();
			public List<History> RecentHistory { get; set; } = new List<History>();
		}

		[Serializable]
		public class Listing
		{
			public string? CreatorID { get; set; }
			public string? CreatotName { get; set; }
			public bool? Hq { get; set; }
			public ulong? LastReviewTime { get; set; }
			public string? ListingId { get; set; }
			////public ?? materia;
			public bool? OnMannequin { get; set; }
			public ulong? PricePerUnit { get; set; }
			public int? Quantity { get; set; }
			public int? RetainerCity { get; set; }
			public string? RetainerID { get; set; }
			public string? RetainerName { get; set; }
			public string? SellerID { get; set; }
			public int? StainID { get; set; }
			public ulong? Total { get; set; }
			public string? UploaderID { get; set; }
			public string? WorldName { get; set; }
		}

		[Serializable]
		public class History
		{
			public string? BuyerName { get; set; }
			public bool? Hq { get; set; }
			public ulong? PricePerUnit { get; set; }
			public int? Quantity { get; set; }
			public string? SellerId { get; set; }
			public ulong? Timestamp { get; set; }
			public ulong? Total { get; set; }
			public string? UploaderID { get; set; }
			public string? WorldName { get; set; }
		}
	}
}
