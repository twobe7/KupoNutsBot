// This document is intended for use by Kupo Nut Brigade developers.

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
