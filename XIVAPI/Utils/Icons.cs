// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

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
