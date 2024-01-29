// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.API;

using System;

[Serializable]
public class Result
{
	public ulong? ID { get; set; }
	public string? Icon { get; set; }
	public string? Name { get; set; }
	public string? Url { get; set; }
	public string? UrlType { get; set; }
}