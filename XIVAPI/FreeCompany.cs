// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class FreeCompany
	{
		public FreeCompany(NetStone.Model.Parseables.FreeCompany.LodestoneFreeCompany freeCompany)
		{
			this.Name = freeCompany.Name;
			this.Tag = freeCompany.Tag;

			if (freeCompany.CrestLayers.BottomLayer != null)
				this.Crest.Add(freeCompany.CrestLayers.BottomLayer.ToString());

			if (freeCompany.CrestLayers.MiddleLayer != null)
				this.Crest.Add(freeCompany.CrestLayers.MiddleLayer.ToString());

			if (freeCompany.CrestLayers.TopLayer != null)
				this.Crest.Add(freeCompany.CrestLayers.TopLayer.ToString());
		}

		public FreeCompany()
		{
		}

		public uint ActiveMemberCount { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Tag { get; set; } = string.Empty;
		public string Slogan { get; set; } = string.Empty;
		public List<string> Crest { get; set; } = new List<string>();
	}
}
