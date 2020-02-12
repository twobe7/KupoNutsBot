// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System.Threading.Tasks;

	public static class ActionAPI
	{
		public static async Task<Action> Get(ulong id)
		{
			string route = "/action/" + id;

			return await Request.Send<Action>(route);
		}
	}
}
