// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Attributes
{
	using System;

	/// <summary>
	/// Replaces the string editor with a drop down of emotes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class InspectorEmoteAttribute : Attribute
	{
		////public readonly Channel.Types ChannelType;

		////public InspectorEmoteAttribute()
		////{
		////	this.ChannelType = Channel.Types.Unknown;
		////}

		////public InspectorEmoteAttribute(Channel.Types type)
		////{
		////	this.ChannelType = type;
		////}
	}
}
