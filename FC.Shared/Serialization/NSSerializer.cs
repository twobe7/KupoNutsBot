// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Serialization
{
	using System;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;

	public static class NSSerializer
	{
		public static JsonSerializerSettings Settings = new JsonSerializerSettings();

		static NSSerializer()
		{
			Settings.ContractResolver = new DefaultContractResolver
			{
				NamingStrategy = new SnakeCaseNamingStrategy(),
			};

			Settings.Formatting = Formatting.Indented;
		}

		public static string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj, Settings);
		}

		public static T Deserialize<T>(string json)
		{
			try
			{
				T deserialised = JsonConvert.DeserializeObject<T>(json, Settings)
					?? throw new NullReferenceException();

				return deserialised;
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
				return JsonConvert.DeserializeObject(json, type, Settings) ?? new object();
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to deserialize JSON: \"" + json + "\" to type: \"" + type + "\"", ex);
			}
		}
	}
}
