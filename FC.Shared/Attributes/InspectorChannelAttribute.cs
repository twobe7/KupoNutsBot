// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Attributes
{
	using System;

	/// <summary>
	/// Replaces the string editor with a drop down of channels.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class InspectorChannelAttribute : Attribute
	{
		public readonly Channel.Types ChannelType;

		public InspectorChannelAttribute(Channel.Types type)
		{
			this.ChannelType = type;
		}
	}
}
