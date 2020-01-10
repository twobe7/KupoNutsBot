// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Manager.Client.RPC
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	[Serializable]
	public class RPCRequest
	{
		public string Method { get; set; } = string.Empty;

		public List<string> ParamData { get; set; } = new List<string>();
	}
}
