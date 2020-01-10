// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class GearSet
	{
		public List<AttributeValue> Attributes { get; set; } = new List<AttributeValue>();

		public GearSetValue? Gear { get; set; }

		public class GearSetValue
		{
			public GearValue? Body { get; set; }

			public GearValue? Bracelets { get; set; }

			public GearValue? Earrings { get; set; }

			public GearValue? Feet { get; set; }

			public GearValue? Hands { get; set; }

			public GearValue? Head { get; set; }

			public GearValue? Legs { get; set; }

			public GearValue? MainHand { get; set; }

			public GearValue? Necklace { get; set; }

			public GearValue? OffHand { get; set; }

			public GearValue? Ring1 { get; set; }

			public GearValue? Ring2 { get; set; }

			public GearValue? SoulCrystal { get; set; }

			public GearValue? Waist { get; set; }
		}

		public class GearValue
		{
			public Item? Item { get; set; }

			public List<Data> Materia { get; set; } = new List<Data>();

			public Data? Mirage { get; set; }
		}
	}
}
