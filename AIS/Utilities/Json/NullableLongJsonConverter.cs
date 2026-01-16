using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIS.Utilities.Json
    {
    public sealed class NullableLongJsonConverter : JsonConverter<long?>
        {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
            if (reader.TokenType == JsonTokenType.Null)
                {
                return null;
                }

            if (reader.TokenType == JsonTokenType.Number)
                {
                if (reader.TryGetInt64(out var longValue))
                    {
                    return longValue;
                    }

                if (reader.TryGetDecimal(out var decimalValue))
                    {
                    return (long)decimalValue;
                    }
                }

            if (reader.TokenType == JsonTokenType.String)
                {
                var text = reader.GetString();
                return NumericParsing.ToNullableLong(text);
                }

            return null;
            }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
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
