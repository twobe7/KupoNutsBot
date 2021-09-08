// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Attributes
{
	using System;

	/// <summary>
	/// Replaces the string editor with a drop down of roles.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class InspectorRoleAttribute : Attribute
	{
		public InspectorRoleAttribute()
		{
		}
	}
}
