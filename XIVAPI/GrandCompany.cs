// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	#pragma warning disable SA1516

	[Serializable]
	public class GrandCompany
	{
		public Data? Company { get; set; }
		public Data? Rank { get; set; }
	}
}
