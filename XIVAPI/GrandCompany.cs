// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class GrandCompany
	{
		public GrandCompany(int companyId, string company, string rankId, string rank)
		{
			this.Company = new Data(id: companyId, name: company);
			this.Rank = new Data(name: rank, icon: rankId);
		}

		public GrandCompany()
		{
		}

		public Data? Company { get; set; }
		public Data? Rank { get; set; }
	}
}
