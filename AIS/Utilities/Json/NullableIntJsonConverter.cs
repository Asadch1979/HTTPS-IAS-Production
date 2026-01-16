using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIS.Utilities.Json
    {
    public sealed class NullableIntJsonConverter : JsonConverter<int?>
        {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
            if (reader.TokenType == JsonTokenType.Null)
                {
                return null;
                }

            if (reader.TokenType == JsonTokenType.Number)
                {
                if (reader.TryGetInt32(out var intValue))
                    {
                    return intValue;
                    }

                if (reader.TryGetDecimal(out var decimalValue))
                    {
                    return (int)decimalValue;
                    }
                }

            if (reader.TokenType == JsonTokenType.String)
                {
                var text = reader.GetString();
                return NumericParsing.ToNullableInt(text);
                }

            return null;
            }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
            {
            if (value.HasValue)
                {
                writer.WriteNumberValue(value.Value);
                return;
                }

            writer.WriteNullValue();
            }
        }
    }
