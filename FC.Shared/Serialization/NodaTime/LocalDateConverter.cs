// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Serialization.NodaTime
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using global::NodaTime;
	using global::NodaTime.Text;

	public class LocalDateConverter : JsonConverter<LocalDate>
	{
		public override LocalDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string id = reader.GetString();
			ParseResult<LocalDate> result = LocalDatePattern.Iso.Parse(id);
			return result.GetValueOrThrow();
		}

		public override void Write(Utf8JsonWriter writer, LocalDate value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(LocalDatePattern.Iso.Format(value));
		}
	}
}
