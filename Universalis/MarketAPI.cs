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

		#pragma warning disable SA1307
		[Serializable]
		public class GetResponse
		{
			public string? dcName;
			public ulong? itemID;
			public ulong? lastUploadTime;
			public List<Listing> listings = new List<Listing>();
		}

		[Serializable]
		public class Listing
		{
			public string? creatorID;
			public string? creatotName;
			public bool? hq;
			public ulong? lastReviewTime;
			public string? listingId;
			////public ?? materia;
			public bool? onMannequin;
			public ulong? pricePerUnit;
			public int? quantity;
			public int? retainerCity;
			public string? retainerID;
			public string? retainerName;
			public string? sellerID;
			public int? stainID;
			public ulong? total;
			public string? uploaderID;
			public string? worldName;
		}

		[Serializable]
		public class History
		{
			public string? buyerName;
			public bool? hq;
			public ulong? pricePerUnit;
			public int? quantity;
			public string? sellerID;
			public ulong? timestamp;
			public ulong? total;
			public string? uploaderID;
			public string? worldName;
		}
	}
}
