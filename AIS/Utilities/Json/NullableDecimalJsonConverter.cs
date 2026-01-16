using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIS.Utilities.Json
    {
    public sealed class NullableDecimalJsonConverter : JsonConverter<decimal?>
        {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
            if (reader.TokenType == JsonTokenType.Null)
                {
                return null;
                }

            if (reader.TokenType == JsonTokenType.Number)
                {
                if (reader.TryGetDecimal(out var decimalValue))
                    {
                    return decimalValue;
                    }
                }

            if (reader.TokenType == JsonTokenType.String)
                {
                var text = reader.GetString();
                return NumericParsing.ToNullableDecimal(text);
                }

            return null;
            }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
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
