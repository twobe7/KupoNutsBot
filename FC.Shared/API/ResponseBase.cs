// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.API;

using System;

[Serializable]
public abstract class ResponseBase
{
	public string? Json { get; set; }
}
