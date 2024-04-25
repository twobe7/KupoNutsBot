// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web.RPC;

using System;

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
