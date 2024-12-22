// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ObjectConvertor : JsonConverter<object>
{
	public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.GetString() ?? string.Empty;
		////switch (reader.TokenType)
		////{
		////	case JsonTokenType.StartObject:
		////		return reader.Read();
		////	case JsonTokenType.False:
		////		return false;
		////	case JsonTokenType.String:
		////		return reader.GetString()?.ToLower() switch
		////		{
		////			"true" => true,
		////			"false" => false,
		////			_ => throw new JsonException(),
		////		};
		////	case JsonTokenType.Number:
		////		ushort number = reader.GetUInt16();
		////		return number == 1;
		////	default:
		////		throw new JsonException();
		////}
	}

	public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
