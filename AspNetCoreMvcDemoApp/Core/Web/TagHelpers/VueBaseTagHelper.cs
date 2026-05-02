using AspNetCoreMvcDemoApp.Core.Web.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCoreMvcDemoApp.Core.Web.TagHelpers
{
    public abstract class VueBaseTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression? For { get; set; }

        [HtmlAttributeName("vue-prefix")]
        public string VuePrefix { get; set; } = "table.filters";

        protected void ProcessVueModel(TagHelperOutput output)
        {
            if (For == null) return;

            var camelName = HtmlExtensions.GetPropertyName(For.Name, useCamelCase: true);

            string vueModelValue = !string.IsNullOrEmpty(VuePrefix) ? $"{VuePrefix}.{camelName}" : camelName;
            output.Attributes.SetAttribute($"v-model", vueModelValue);

            // Clean up the custom attributes to avoid rendering them in the final HTML
            output.Attributes.RemoveAll("vue-model");
            output.Attributes.RemoveAll("vue-prefix");

            // Clear the content of the element to prevent any inner HTML from being rendered
            // stop __Invariant elements from rendering their content, which is not needed for Vue components
            output.PostElement.Clear();
        }
    }
}
