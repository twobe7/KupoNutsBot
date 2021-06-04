// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;
	using Microsoft.AspNetCore.Components;

	public static class Roles
	{
		public static async Task<List<Role>> GetRoles()
		{
			List<Role> allRoles = await RPCService.Invoke<List<Role>>("GuildService.GetRoles");

			List<Role> roles = new List<Role>();
			foreach (Role role in allRoles)
			{
				roles.Add(role);
			}

			roles.Sort((Role a, Role b) =>
			{
				return a.Name.CompareTo(b.Name);
			});

			return roles;
		}
	}
}
