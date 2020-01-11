// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client.RPC
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	[Serializable]
	public class RPCRequest
	{
		public string Token { get; set; } = string.Empty;

		public string GuildId { get; set; } = string.Empty;

		public string Method { get; set; } = string.Empty;

		public List<string> ParamData { get; set; } = new List<string>();
	}
}
