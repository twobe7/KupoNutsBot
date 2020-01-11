// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.RPC
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;
	using Newtonsoft.Json;

	/// <summary>
	/// Guild RPC's check that the incoming RPC requester has the permission to access the given guild.
	/// the first parameter of a Guild RPC must be the guildId as a string (string guildId).
	/// </summary>
	public class GuildRpcAttribute : RPCAttribute
	{
		public override List<object> GetParameters(RPCRequest request, MethodInfo method, List<string> paramData)
		{
			// guild Id is first parameter, and since its a string, we can just insert it into the JSON data here.
			paramData.Insert(0, request.GuildId);
			return base.GetParameters(request, method, paramData);
		}

		public override bool Authenticate(RPCRequest request)
		{
			string guildId = request.GuildId;

			// TODO:
			return true;
		}
	}
}
