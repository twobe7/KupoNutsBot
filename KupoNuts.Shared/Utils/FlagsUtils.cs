// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class FlagsUtils
	{
		public static bool IsSet<T>(T flags, T flag)
			where T : struct
		{
			int flagsValue = (int)(object)flags;
			int flagValue = (int)(object)flag;

			return (flagsValue & flagValue) != 0;
		}

		public static void Set<T>(ref T flags, T flag)
			where T : struct
		{
			int flagsValue = (int)(object)flags;
			int flagValue = (int)(object)flag;

			flags = (T)(object)(flagsValue | flagValue);
		}

		public static void Unset<T>(ref T flags, T flag)
			where T : struct
		{
			int flagsValue = (int)(object)flags;
			int flagValue = (int)(object)flag;

			flags = (T)(object)(flagsValue & (~flagValue));
		}
	}
}
