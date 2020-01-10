// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Text;

	public static class PathUtils
	{
		public static string Current
		{
			get
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return "./FCBot/bin/";

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return "./";

				throw new PlatformNotSupportedException();
			}
		}
	}
}
