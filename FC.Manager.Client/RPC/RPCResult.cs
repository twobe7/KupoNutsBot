// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client.RPC
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	public class RPCResult
	{
		public RPCResult()
		{
		}

		public RPCResult(Exception ex)
		{
			this.Exception = ex.Message;
		}

		public string Data { get; set; }

		public string Exception { get; set; }
	}
}
