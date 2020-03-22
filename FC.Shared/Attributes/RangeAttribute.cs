// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class RangeAttribute : Attribute
	{
		public readonly double Min;
		public readonly double Max;

		public RangeAttribute(double min, double max)
		{
			this.Min = min;
			this.Max = max;
		}
	}
}
