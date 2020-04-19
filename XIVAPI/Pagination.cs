// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;

	[Serializable]
	public class Pagination
	{
		public int Page { get; set; }
		public int PageTotal { get; set; }
		public int Results { get; set; }
		public int ResultsPerPage { get; set; }
		public int ResultsTotal { get; set; }
	}
}
