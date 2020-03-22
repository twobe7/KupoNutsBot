// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Attributes
{
	using System;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class InspectorTooltipAttribute : Attribute
	{
		public readonly string Content;

		public InspectorTooltipAttribute(string content)
		{
			this.Content = content;
		}
	}
}
