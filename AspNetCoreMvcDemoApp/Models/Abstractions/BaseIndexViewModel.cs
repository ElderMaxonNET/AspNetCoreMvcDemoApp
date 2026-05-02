using AspNetCoreMvcDemoApp.Core.Web.Extensions;
using Microsoft.AspNetCore.Html;

namespace AspNetCoreMvcDemoApp.Models.Abstractions
{
    public abstract class BaseIndexViewModel<TListDto, TSearchDto>(TableViewContext<TListDto, TSearchDto> context)
        where TListDto : class, new()
        where TSearchDto : BaseSearchDto, new()
    {
        public TableViewContext<TListDto, TSearchDto> Context { get; } = context;

        public IHtmlContent ToRawJson()
        {
            var columns = Context.Data.ExtractColumns();
            var pk = columns.FirstOrDefault(x => x.IsKey)?.Name ?? "id";

            var jData = new
            {
                data = Context.Data,
                columns = columns,
                searchModel = Context.Search,
                loadUrl = Context.LoadUrl,
                primaryKey = pk
            };

            return jData.SerializeToRawJson();
        }
    }
}
