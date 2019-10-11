// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System.Threading.Tasks;

	public static class ItemAPI
	{
		public static async Task<Item> Get(ulong id)
		{
			string route = "/item/" + id;

			return await Request.Send<Item>(route);
		}
	}
}
