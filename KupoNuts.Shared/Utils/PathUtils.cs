// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Utils
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
					return "./KupoNutsBot/bin/";

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return "./";

				throw new PlatformNotSupportedException();
			}
		}
	}
}
