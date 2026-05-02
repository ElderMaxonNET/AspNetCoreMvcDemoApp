using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetCoreMvcDemoApp.Core.Infrastructure.Json
{
    public class SmartDateTimeConverter : JsonConverter<object>
    {
        private const string DateTimeFormat = "dd.MM.yyyy HH:mm";
        private const string DateOnlyFormat = "dd.MM.yyyy";

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTime) ||
                   typeToConvert == typeof(DateTime?) ||
                   typeToConvert == typeof(DateOnly) ||
                   typeToConvert == typeof(DateOnly?);
        }

        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value)) return null;

            if (typeToConvert == typeof(DateOnly) || typeToConvert == typeof(DateOnly?))
                return DateOnly.Parse(value);

            return DateTime.Parse(value);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case DateTime dt:
                    writer.WriteStringValue(dt.ToString(DateTimeFormat));
                    break;
                case DateOnly d:
                    writer.WriteStringValue(d.ToString(DateOnlyFormat));
                    break;
                default:
                    writer.WriteNullValue();
                    break;
            }
        }
    }
}
