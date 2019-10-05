// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Utils
{
	public static class Characters
	{
		public const string Space = " ឵";
		public const string Tab = Space + Space + Space + Space;
		public const string DoubleTab = Tab + Tab;

		public static int GetWidth(string str)
		{
			int width = 0;
			foreach (char character in str)
				width += GetWidth(character);

			return width;
		}

		public static int GetWidth(char character)
		{
			switch (character)
			{
				case '0': return 4;
			}

			if (char.IsNumber(character))
				return 3;

			if (char.IsUpper(character))
				return 4;

			return 2;
		}
	}
}
