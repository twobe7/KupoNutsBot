// This document is intended for use by Kupo Nut Brigade developers.

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
