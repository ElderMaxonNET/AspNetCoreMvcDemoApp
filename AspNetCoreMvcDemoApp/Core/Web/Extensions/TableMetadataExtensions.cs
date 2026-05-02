namespace AspNetCoreMvcDemoApp.Core.Web.Extensions
{
    using SadLib.Data.Abstractions;
    using SadLib.Data.Providers;
    using System.Collections.Concurrent;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using System.Text.Json;

    public record ColumnMetadata(string Name, string PropName, string DisplayName, bool IsKey = false);

    public static class TableMetadataExtensions
    {
        private static readonly ConcurrentDictionary<Type, List<ColumnMetadata>> _metadataCache = new();

        public static List<ColumnMetadata> ExtractColumns(this Type type)
        {
            return _metadataCache.GetOrAdd(type, t =>
            {
                    return [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => (
                            Prop: p,
                            Display: p.GetCustomAttribute<DisplayAttribute>(),
                            Key: p.GetCustomAttribute<KeyAttribute>())
                        )
                    .Where(x => x.Display != null && x.Display.GetAutoGenerateField() != false)
                    .OrderBy(x => x.Display!.GetOrder() ?? 1000)
                    .Select(x => {
                        return new ColumnMetadata(
                            Name: JsonNamingPolicy.CamelCase.ConvertName(x.Prop.Name),
                            PropName: x.Prop.Name,
                            DisplayName: x.Display!.GetName() ?? x.Prop.Name,
                            IsKey: x.Key != null
                        );
                    })];
            });
        }

        public static List<ColumnMetadata> ExtractColumns<T>(this IPagedResult<T> _) where T : class, new()
        {
            return typeof(T).ExtractColumns();
        }

        public static List<ColumnMetadata> ExtractColumns<T>(this IEnumerable<T> _) where T : class, new()
        {
            return typeof(T).ExtractColumns();
        }
    }
}
