// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace PaissaHouse
{
    using Newtonsoft.Json;
    using System;
	using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;

	public class HousingAPI
	{
		private enum WorldEnum
		{
			Kujata = 49,
			Typhon = 50,
			Tonberry = 72,
			Adamantoise = 73,
		}

		private enum DistrictEnum
		{
			Mist = 339,
			[Description("The Lavender Beds")]
			TheLavenderBeds = 340,
			[Description("The Goblet")]
			TheGoblet = 341,
			Shirogane = 641,
		}

		public static async Task<SearchResponse> Worlds(string name)
		{
			// Convert world name to enum for ID, no results if we cannot parse name
			if (!Enum.TryParse(name, true, out WorldEnum world))
				return new SearchResponse();

			string route = "/worlds/" + (int) world;

			SearchResponse response = await Request.Send<SearchResponse>(route);

			if (response == null)
				return new SearchResponse();

			return response;
		}

		[Serializable]
		public class SearchResponse : ResponseBase
		{
			public string Name { get; set; }
			public List<District> Districts { get; set; }
			public uint NumOpenPlots { get; set; }
			public DateTime OldestPlotTime { get; set; }
		}

		[Serializable]
		public class District
		{
			public uint Id { get; set; }
			public string Name { get; set; }
			public uint NumOpenPlots { get; set; }
			public DateTime OldestPlotTime { get; set; }
			public List<OpenPlot> OpenPlots { get; set; }
		}

			[Serializable]
		public class OpenPlot
		{
			public uint WorldId { get; set; }
			public uint DistrictId { get; set; }
			public uint WardNumber { get; set; }
			public uint PlotNumber { get; set; }
			public uint Size { get; set; }
			public ulong KnownPrice { get; set; }
			public DateTime LastUpdatedTime { get; set; }
			public DateTime EstTimeOpenMin { get; set; }
			public DateTime EstTimeOpenMax { get; set; }
			public uint EstNumDevals { get; set; }

			public string GetInfo => $"Ward: {this.WardNumber}. Plot: {this.PlotNumber}. Size: {this.Size}. Price: **{this.KnownPrice.ToString("N0")}**. Last Updated: {this.LastUpdatedTime.ToString("dd/MM HH:ss")}";
		}
	}
}
