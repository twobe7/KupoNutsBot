// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Attributes
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class InspectorHiddenAttribute : Attribute
	{
	}
}
