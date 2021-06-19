// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace System
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Text.RegularExpressions;

	public static class EnumExtensions
	{
		public static readonly Regex Regex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

		public static string ToDisplayString(this Enum value)
		{
			Type type = value.GetType();
			string name = Enum.GetName(type, value);
			if (name != null)
			{
				FieldInfo field = type.GetField(name);
				if (field != null)
				{
					DescriptionAttribute? attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
					return attr != null ? attr.Description : Regex.Replace(field.Name, " ");
				}
			}

			return string.Empty;
		}

		public static string[] GetDisplayStrings(this Type enumType)
		{
			List<string> list = new List<string>();
			foreach (object value in enumType.GetEnumValues())
			{
				string name = Enum.GetName(enumType, value);
				if (name != null)
				{
					FieldInfo field = enumType.GetField(name);
					if (field != null)
					{
						DescriptionAttribute? attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
						list.Add(attr != null ? attr.Description : Regex.Replace(field.Name, " "));
					}
				}
			}

			return list.ToArray();
		}
	}
}
