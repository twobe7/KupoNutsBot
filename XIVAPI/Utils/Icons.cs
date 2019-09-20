// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	public static class Icons
	{
		public static string? GetIconURL(string? iconPath)
		{
			if (string.IsNullOrEmpty(iconPath))
				return null;

			return "https://xivapi.com/" + iconPath;
		}
	}
}
