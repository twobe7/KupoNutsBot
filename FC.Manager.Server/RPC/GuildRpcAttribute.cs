// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.RPC
{
	using System.Collections.Generic;
	using System.Reflection;
	using FC.Manager.Client.RPC;

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

			// NOTE: the token comes from the client, so it can be tampered with, however it should be
			// cryptographically hard to generate a new valid token.
			// we _could_ check the server to see if the user truly has access to the guild as a second layer of security,
			// but the incoming user id could also be faked.
			return Authentication.VerifyToken(request.Token, guildId);
		}
	}
}
