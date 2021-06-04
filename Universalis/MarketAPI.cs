// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Universalis
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	public static class MarketAPI
	{
		public static async Task<GetResponse> Get(string dataCenter, ulong itemId)
		{
			return await Request.Send<GetResponse>("/" + dataCenter + "/" + itemId);
		}

		public static async Task<IOrderedEnumerable<History>> GetBestPriceFromAllWorlds(string dataCenter, ulong itemId)
		{
			GetResponse response = await Get(dataCenter, itemId);

			List<History> results = new List<History>();

			foreach (IGrouping<string?, History> worldGroup in response.RecentHistory.GroupBy(x => x.WorldName))
			{
				string worldName = worldGroup.Key ?? string.Empty;

				ulong? bestHqPrice = ulong.MaxValue;
				History? bestHq = null;

				ulong? bestNmPrice = ulong.MaxValue;
				History? bestNm = null;

				foreach (History entry in worldGroup)
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

				if (bestHq != null)
				{
					results.Add(bestHq);
				}

				if (bestNm != null)
				{
					results.Add(bestNm);
				}
			}

			return results.OrderBy(x => x.PricePerUnit);
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

		public static async Task<IOrderedEnumerable<ListingDisplay>> GetBestPriceTest(string dataCenter, ulong itemId, bool? hqOnly, bool lowestByUnitPrice)
		{
			GetResponse response = await Get(dataCenter, itemId);

			List<ListingDisplay> results = new List<ListingDisplay>();

			// Filter for quality
			if (hqOnly.HasValue)
				response.Listings = response.Listings.Where(x => x.Hq == hqOnly).ToList();

			foreach (IGrouping<string?, Listing> worldGroup in response.Listings.GroupBy(x => x.WorldName))
			{
				string? bestListingId = null;
				ulong? bestPrice = ulong.MaxValue;

				foreach (Listing entry in worldGroup)
				{
					if (entry.PricePerUnit == null)
						continue;

					if (lowestByUnitPrice)
					{
						if (entry.PricePerUnit < bestPrice)
						{
							bestListingId = entry.ListingId;
						}
					}
					else
					{
						if (entry.Total < bestPrice)
						{
							bestListingId = entry.ListingId;
						}
					}
				}

				if (bestListingId != null)
					results.Add(MapToHistoryDisplay(worldGroup.FirstOrDefault(x => x.ListingId == bestListingId)));
			}

			return results.OrderBy(x => lowestByUnitPrice ? x.PricePerUnit : x.Total);
		}

		private static ListingDisplay MapToHistoryDisplay(Listing listing)
		{
			return new ListingDisplay()
			{
				WorldName = listing.WorldName,
				Quantity = listing.Quantity,
				PricePerUnit = listing.PricePerUnit,
				Total = listing.Total,
				Hq = listing.Hq,
			};
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
			public string? CreatorName { get; set; }
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

		public class ListingDisplay
		{
			private const ulong MaxGil = 999999999;

			public string? WorldName { get; set; }
			public int? Quantity { get; set; }
			public ulong? PricePerUnit { get; set; }
			public ulong? Total { get; set; }
			public bool? Hq { get; set; }

			public string MaxPricePerUnit
			{
				get
				{
					ulong? ppu = this.PricePerUnit > MaxGil ? MaxGil : this.PricePerUnit;
					return ppu?.ToString("N0") + "g";
				}
			}

			public string MaxTotal
			{
				get
				{
					ulong? total = this.Total > MaxGil ? MaxGil : this.Total;
					return total?.ToString("N0") + "g";
				}
			}
		}
	}
}
