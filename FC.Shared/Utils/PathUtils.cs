// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Utils
{
	using System.IO;
	using System.Reflection;

	public static class PathUtils
	{
		public static string Current
		{
			get
			{
#if DEBUG
				return $"{Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)}";
#else
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return "./FCChanBot/bin/";

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return "./";

				throw new PlatformNotSupportedException();
#endif
			}
		}
	}
}
