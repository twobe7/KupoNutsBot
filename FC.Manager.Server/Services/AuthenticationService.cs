// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System.Threading.Tasks;

	public class AuthenticationService : ServiceBase
	{
		[RPC]
		public string GetDiscordKey()
		{
			Settings settings = Settings.Load();
			return settings.DiscordKey;
		}
	}
}
