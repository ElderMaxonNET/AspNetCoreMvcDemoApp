using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCoreMvcDemoApp.Core.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-for, vue-model")]
    public class VueGenericTagHelper : VueBaseTagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessVueModel(output);
        }
    }
}
