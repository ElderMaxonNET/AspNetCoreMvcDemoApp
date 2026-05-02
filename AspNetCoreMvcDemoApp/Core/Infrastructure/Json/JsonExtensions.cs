using Microsoft.AspNetCore.Html;
using System.Text.Json;

namespace AspNetCoreMvcDemoApp.Core.Infrastructure.Json
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Converters = { new SmartDateTimeConverter() }
        };

        public static IHtmlContent SerializeToRawJson<T>(this T? model)
        {
            if (model == null)
                return new HtmlString("null");

            return new HtmlString(JsonSerializer.Serialize(model, _options));
        }

        public static IHtmlContent SerializeToRawJson(this object? model)
        {
            if (model == null)
                return new HtmlString("null");

            return new HtmlString(JsonSerializer.Serialize(model, _options));
        }
    }
}
