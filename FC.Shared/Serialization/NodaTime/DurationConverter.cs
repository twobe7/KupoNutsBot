// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Serialization.NodaTime;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using global::NodaTime;
using global::NodaTime.Text;

public class DurationConverter : JsonConverter<Duration>
{
	public override Duration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string id = reader.GetString() ?? string.Empty;
		return DurationPattern.Roundtrip.Parse(id).GetValueOrThrow();
	}

	public override void Write(Utf8JsonWriter writer, Duration value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(DurationPattern.Roundtrip.Format(value));
	}
}
