// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Attributes
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// Replaces the string editor with a drop down of channels.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class InspectorTimeZoneAttribute : Attribute
	{
		public readonly List<TimeZoneInfo> Timezone;

		public InspectorTimeZoneAttribute()
		{
			this.Timezone = TimeZoneInfo.GetSystemTimeZones().ToList();
		}

		public InspectorTimeZoneAttribute(List<TimeZoneInfo> timeZones)
		{
			this.Timezone = timeZones;
		}
	}
}
