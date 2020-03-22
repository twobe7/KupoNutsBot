// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Serialization
{
	using System;
	using System.Reflection;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using FC.Serialization.NodaTime;

	public static class Serializer
	{
		public static JsonSerializerOptions Options = new JsonSerializerOptions();

		static Serializer()
		{
			Options.WriteIndented = true;
			Options.PropertyNameCaseInsensitive = true;
			Options.Converters.Add(new DateTimeZoneConverter());
		}

		public static string Serialize<T>(T obj)
		{
			return JsonSerializer.Serialize<T>(obj, Options);
		}

		public static string Serialize(object obj)
		{
			return JsonSerializer.Serialize(obj, Options);
		}

		public static T Deserialize<T>(string json)
		{
			try
			{
				return JsonSerializer.Deserialize<T>(json, Options);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to deserialize JSON: \"" + json + "\" to type: \"" + typeof(T) + "\"", ex);
			}
		}

		public static object Deserialize(string json, Type type)
		{
			try
			{
				return JsonSerializer.Deserialize(json, type, Options);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to deserialize JSON: \"" + json + "\" to type: \"" + type + "\"", ex);
			}
		}
	}
}
