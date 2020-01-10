// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Controllers
{
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;
	using Microsoft.AspNetCore.Mvc;

	using RPCService = FC.Manager.Server.Services.RPCService;

	[ApiController]
	[Route("[controller]")]
	public class RPCController : ControllerBase
	{
		[HttpPost]
		public async Task<RPCResult> Post(RPCRequest req)
		{
			return await RPCService.Invoke(req);
		}
	}
}
