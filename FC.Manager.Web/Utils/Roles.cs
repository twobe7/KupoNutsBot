// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web
{
	using System.Collections.Generic;
	using FC.Manager.Web.Services;

	public static class Roles
	{
		public static List<Role> GetRoles(
			GuildService guildService)
		{
			List<Role> allRoles = guildService.GetRoles(RPCService.GuildId);

			allRoles.Sort((Role a, Role b) =>
			{
				return a.Name.CompareTo(b.Name);
			});

			return allRoles;
		}
	}
}
