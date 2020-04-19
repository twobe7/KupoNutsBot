// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Serialization.NodaTime
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using global::NodaTime;

	public class DateTimeZoneConverter : JsonConverter<DateTimeZone>
	{
		public override DateTimeZone Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string id = reader.GetString();
			return DateTimeZoneProviders.Tzdb.GetZoneOrNull(id);
		}

		public override void Write(Utf8JsonWriter writer, DateTimeZone value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Id);
		}
	}
}
