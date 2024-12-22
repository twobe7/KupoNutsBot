// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Serialization
{
	using System;
	using System.Text.Json;
	using FC.Serialization.NodaTime;

	public static class Serializer
	{
		public static JsonSerializerOptions Options = new()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true,
		};

		public static JsonSerializerOptions SnakeCaseOptions = new()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
		};

		static Serializer()
		{
			Options.Converters.Add(new DateTimeZoneConverter());
			Options.Converters.Add(new LocalDateConverter());
			Options.Converters.Add(new DurationConverter());
		}

		public static string Serialize<T>(T obj)
		{
			return JsonSerializer.Serialize<T>(obj, Options);
		}

		public static string Serialize(object obj)
		{
			return JsonSerializer.Serialize(obj, Options);
		}

		public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
		{
			try
			{
				options ??= Options;
				return JsonSerializer.Deserialize<T>(json, options);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to deserialize JSON: \"" + json + "\" to type: \"" + typeof(T) + "\"", ex);
			}
		}

		public static T? DeserializeResponse<T>(string json, JsonSerializerOptions? options = null)
			where T : API.ResponseBase
		{
			try
			{
				options ??= Options;
				T? result = JsonSerializer.Deserialize<T>(json, options);

				if (result != null)
					result.Json = json;

				return result;
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
				return JsonSerializer.Deserialize(json, type, Options) ?? new object();
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to deserialize JSON: \"" + json + "\" to type: \"" + type + "\"", ex);
			}
		}
	}
}
