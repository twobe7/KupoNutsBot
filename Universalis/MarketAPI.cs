// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Universalis
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using FC.XIVData;

	public static class MarketAPI
	{
		public static async Task<GetResponse> Get(string dataCenter, ulong itemId)
		{
			return await Request.Send<GetResponse>($"/{dataCenter}/{itemId}");
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

		public static async Task<(History?, History?)> GetBestPriceHistory(string dataCenter, ulong itemId)
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

		public static async Task<IOrderedEnumerable<ListingDisplay>> GetBestPriceListing(string dataCenter, ulong itemId, bool? hqOnly, bool lowestByUnitPrice)
		{
			Dictionary<string, ulong> worldLastUpdated = new Dictionary<string, ulong>();
			List<Listing> dataCentreListings = new List<Listing>();
			List<ListingDisplay> results = new List<ListingDisplay>();

			if (World.ServerDataCentreLookup.TryGetValue(dataCenter, out var dataCentreWorlds))
			{
				foreach (string world in dataCentreWorlds)
				{
					GetResponse response = await Get(world, itemId);
					if (response?.Listings != null)
					{
						// Update world name inside listing
						response.Listings.ForEach(x => x.WorldName = world);

						// Add listings
						dataCentreListings.AddRange(response.Listings);

						// Add world last updated time to dictionary
						if (response.LastUploadTime.HasValue)
							worldLastUpdated.TryAdd(world, response.LastUploadTime.Value);
					}
				}
			}
			else
			{
				GetResponse response = await Get(dataCenter, itemId);
				if (response?.Listings != null)
					dataCentreListings.AddRange(response.Listings);
			}

			// Filter for quality
			if (hqOnly.HasValue)
				dataCentreListings = dataCentreListings.Where(x => x.Hq == hqOnly).ToList();

			foreach (IGrouping<string?, Listing> worldGroup in dataCentreListings.GroupBy(x => x.WorldName))
			{
				if (worldGroup.Key == null)
					continue;

				Listing? bestListing = null;

				// Get listings with a price per unit
				IEnumerable<Listing> listings = worldGroup.Where(x => x.PricePerUnit != null);

				// If searching by lowest Unit Price, get lowest PricePerUnit otherwise lowest Total
				bestListing = lowestByUnitPrice
						? listings.OrderBy(x => x.PricePerUnit).GetFirst()
						: listings.OrderBy(x => x.Total).GetFirst();

				// If found a listing, map to history
				if (bestListing != null)
				{
					var lastUpdatedOrReviewed = worldLastUpdated.TryGetValue(worldGroup.Key, out ulong lastUpdated) ? lastUpdated : (bestListing.LastReviewTimeMilliseconds ?? GetDefaultLastUpdated());
					results.Add(MapToHistoryDisplay(bestListing, lastUpdatedOrReviewed));
				}
			}

			return results.OrderBy(x => lowestByUnitPrice ? x.PricePerUnit : x.Total);
		}

		private static ListingDisplay MapToHistoryDisplay(Listing listing, ulong worldLastUpdated)
		{
			return new ListingDisplay
			{
				WorldName = listing.WorldName,
				Quantity = listing.Quantity,
				PricePerUnit = listing.PricePerUnit,
				Total = listing.Total,
				Hq = listing.Hq,
				LastUpdated = worldLastUpdated,
			};
		}

		private static ulong GetDefaultLastUpdated()
		{
			return Convert.ToUInt64(DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeMilliseconds());
		}

		[Serializable]
		public class GetResponse
		{
			public string? DcName { get; set; }
			public ulong? ItemID { get; set; }

			/// <summary>
			/// Gets or sets the last upload time for this endpoint, in milliseconds since the UNIX epoch.
			/// </summary>
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

			/// <summary>
			///  Gets or sets the time that this listing was posted, in seconds since the UNIX epoch.
			/// </summary>
			public ulong? LastReviewTime { get; set; }
			public string? ListingId { get; set; }
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

			public ulong? LastReviewTimeMilliseconds => this.LastReviewTime * 1000;
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
			public ulong? LastUpdated { get; set; }

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

			public string LastUpdatedIcon
			{
				get
				{
					var now = Convert.ToUInt64(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
					var diff = now - this.LastUpdated;

					if (diff < new TimeSpan(1, 0, 0).TotalMilliseconds)
					{
						return ":green_circle:";
					}
					else if (diff < new TimeSpan(24, 0, 0).TotalMilliseconds)
					{
						return ":orange_circle:";
					}

					return ":red_circle:";
				}
			}
		}
	}
}
