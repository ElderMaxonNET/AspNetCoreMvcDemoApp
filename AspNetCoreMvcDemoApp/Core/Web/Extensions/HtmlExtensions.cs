using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace AspNetCoreMvcDemoApp.Core.Web.Extensions
{
    public static class HtmlExtensions
    {
        private static readonly ConcurrentDictionary<string, (string Camel, string Original)> _globalNameCache = new();

        public static string GetPropertyName(string fullPath, bool useCamelCase = true)
        {
            if (string.IsNullOrEmpty(fullPath)) return string.Empty;

            var propertyName = fullPath.Split('.').Last();

            var (Camel, Original) = _globalNameCache.GetOrAdd(propertyName, key =>
            {
                var original = key;
                var camel = string.Create(original.Length, original, (span, value) =>
                {
                    value.AsSpan().CopyTo(span);
                    if (span.Length > 0 && char.IsUpper(span[0]))
                    {
                        span[0] = char.ToLowerInvariant(span[0]);
                    }
                });
                return (camel, original);
            });

            return useCamelCase ? Camel : Original;
        }

        private static string ResolvePathFromExpression<TModel, TProperty>(IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var expressionProvider = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<ModelExpressionProvider>();
            return expressionProvider.GetExpressionText(expression);
        }

        public static string CamelCaseNameFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var path = ResolvePathFromExpression(htmlHelper, expression);
            return GetPropertyName(path, useCamelCase: true);
        }

        public static string PropertyNameFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var path = ResolvePathFromExpression(htmlHelper, expression);
            return GetPropertyName(path, useCamelCase: false);
        }
    } 
}
