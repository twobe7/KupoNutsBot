// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;

	public static class Authentication
	{
		public const string DiscordScopes = "identify guilds";

		private static Data data;

		public static bool IsAuthenticated
		{
			get
			{
				return !string.IsNullOrEmpty(data?.AuthToken);
			}
		}

		public static string Token
		{
			get
			{
				return data?.AuthToken;
			}
		}

		public static List<Data.Guild> Guilds
		{
			get
			{
				return data?.Guilds;
			}
		}

		public static async Task Authenticate(string code, string url)
		{
			data = await RPCService.Invoke<Data>("AuthenticationService.AuthenticateCode", url, code);

			if (data == null)
				throw new Exception("Authentication failed");

			if (string.IsNullOrEmpty(data.AuthToken))
				throw new Exception("Invalid token");

			if (data.Guilds == null || data.Guilds.Count <= 0)
				throw new Exception("You must be in at least one guild");

			// set the first available guild as the default
			foreach (Data.Guild guild in data.Guilds)
			{
				if (!guild.CanManageGuild)
					continue;

				RPCService.GuildId = guild.Id;
				break;
			}

			if (string.IsNullOrEmpty(RPCService.GuildId))
			{
				throw new Exception("you must have the 'Manage Guild' permission on at least one guild");
			}
		}

		[Serializable]
		public class Data
		{
			public string DiscordUserId { get; set; }
			public string DiscordUserName { get; set; }
			public string AuthToken { get; set; }
			public List<Guild> Guilds { get; set; }

			public class Guild
			{
				public const int AdministratorPermission = 0x00000008;
				public const int ManageGuildPermission = 0x00000020;

				public string Id { get; set; }
				public string Name { get; set; }
				public string Icon { get; set; }
				public bool Owner { get; set; }
				public int Permissions { get; set; }

				public bool IsAdministrator
				{
					get
					{
						return (this.Permissions & AdministratorPermission) == AdministratorPermission;
					}
				}

				public bool CanManageGuild
				{
					get
					{
						return (this.Permissions & ManageGuildPermission) == ManageGuildPermission;
					}
				}
			}
		}
	}
}
