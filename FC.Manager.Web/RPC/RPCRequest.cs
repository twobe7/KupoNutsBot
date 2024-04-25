// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web.RPC;

using System;
using System.Collections.Generic;

[Serializable]
public class RPCRequest
{
	public string Token { get; set; } = string.Empty;

	public ulong GuildId { get; set; } = 0;

	public string Method { get; set; } = string.Empty;

	public List<string> ParamData { get; set; } = new List<string>();
}
