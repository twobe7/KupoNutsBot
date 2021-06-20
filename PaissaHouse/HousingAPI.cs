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

		private enum SizeEnum
		{
			S = 0,
			M = 1,
			L = 2,
		}

		/// <summary>
		/// Housing grades for The Goblet
		/// </summary>
		private static readonly Dictionary<uint, uint> TheGobletGrades = new Dictionary<uint, uint>
		{
			{ 1, 1 }, { 2, 4 }, { 3, 3 }, { 4, 2 }, { 5, 4 },
			{ 6, 4 }, { 7, 4 }, { 8, 3 }, { 9, 5 }, { 10, 5 },
			{ 11, 5 }, { 12, 5 }, { 13, 4 }, { 14, 1 }, { 15, 1 },
			{ 16, 2 }, { 17, 1 }, { 18, 1 }, { 19, 1 }, { 20, 1 },
			{ 21, 4 }, { 22, 5 }, { 23, 3 }, { 24, 4 }, { 25, 5 },
			{ 26, 5 }, { 27, 2 }, { 28, 4 }, { 29, 4 }, { 30, 2 },
		};

		/// <summary>
		/// Housing grades for The Lavender Beds
		/// </summary>
		private static readonly Dictionary<uint, uint> TheLavenderBedsGrades = new Dictionary<uint, uint>
		{
			{ 1, 3 }, { 2, 5 }, { 3, 2 }, { 4, 5 }, { 5, 5 },
			{ 6, 5 }, { 7, 4 }, { 8, 3 }, { 9, 2 }, { 10, 2 },
			{ 11, 1 }, { 12, 4 }, { 13, 2 }, { 14, 5 }, { 15, 4 },
			{ 16, 1 }, { 17, 1 }, { 18, 5 }, { 19, 4 }, { 20, 4 },
			{ 21, 4 }, { 22, 1 }, { 23, 3 }, { 24, 1 }, { 25, 3 },
			{ 26, 2 }, { 27, 2 }, { 28, 2 }, { 29, 1 }, { 30, 2 },
		};

		/// <summary>
		/// Housing grades for The Mist
		/// </summary>
		private static readonly Dictionary<uint, uint> MistGrades = new Dictionary<uint, uint>
		{
			{ 1, 2 }, { 2, 2 }, { 3, 4 }, { 4, 1 }, { 5, 1 },
			{ 6, 1 }, { 7, 5 }, { 8, 1 }, { 9, 3 }, { 10, 3 },
			{ 11, 4 }, { 12, 4 }, { 13, 2 }, { 14, 4 }, { 15, 2 },
			{ 16, 5 }, { 17, 5 }, { 18, 5 }, { 19, 1 }, { 20, 1 },
			{ 21, 1 }, { 22, 3 }, { 23, 5 }, { 24, 5 }, { 25, 4 },
			{ 26, 3 }, { 27, 5 }, { 28, 4 }, { 29, 4 }, { 30, 1 },
		};

		/// <summary>
		/// Housing grades for Shirogane
		/// </summary>
		private static readonly Dictionary<uint, uint> ShiroganeGrades = new Dictionary<uint, uint>
		{
			{ 1, 1 }, { 2, 2 }, { 3, 1 }, { 4, 2 }, { 5, 3 },
			{ 6, 2 }, { 7, 4 }, { 8, 1 }, { 9, 2 }, { 10, 4 },
			{ 11, 4 }, { 12, 4 }, { 13, 4 }, { 14, 5 }, { 15, 3 },
			{ 16, 5 }, { 17, 1 }, { 18, 2 }, { 19, 1 }, { 20, 2 },
			{ 21, 1 }, { 22, 1 }, { 23, 1 }, { 24, 2 }, { 25, 3 },
			{ 26, 4 }, { 27, 5 }, { 28, 5 }, { 29, 4 }, { 30, 3 },
		};

		private static uint GetPlotGrade(DistrictEnum district, uint plot)
		{
			uint grade = 1;

			switch (district)
			{
				case DistrictEnum.Mist:
					if (!MistGrades.TryGetValue(plot, out grade))
						MistGrades.TryGetValue(plot - 30, out grade);
					break;
				case DistrictEnum.Shirogane:
					if (!ShiroganeGrades.TryGetValue(plot, out grade))
						ShiroganeGrades.TryGetValue(plot - 30, out grade);
					break;
				case DistrictEnum.TheGoblet:
					if (!TheGobletGrades.TryGetValue(plot, out grade))
						TheGobletGrades.TryGetValue(plot - 30, out grade);
					break;
				case DistrictEnum.TheLavenderBeds:
					if (!TheLavenderBedsGrades.TryGetValue(plot, out grade))
						TheLavenderBedsGrades.TryGetValue(plot - 30, out grade);
					break;
			}

			return grade;
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

			private DistrictEnum District => (DistrictEnum)DistrictId;

			private uint Grade => GetPlotGrade(this.District, this.PlotNumber);

			private string KnownPriceMillions
			{
				get
				{
					return (this.KnownPrice / 1000000M).ToString("N3") + "m";
				}
			}

			public string GetInfo()
			{
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				builder.Append("• ");
				builder.Append($"Ward: {this.WardNumber}. ");
				builder.Append($"Plot: {this.PlotNumber}. ");
				builder.Append($"Grade: {this.Grade}. ");
				builder.Append($"Size: {((SizeEnum)this.Size).ToDisplayString()}. ");
				builder.Append($"Price: **{this.KnownPriceMillions}**. ");
				builder.Append($"Last Updated: {this.LastUpdatedTime.ToString("dd/MM HH:ss")}. ");

				return builder.ToString();
			}
		}
	}
}
