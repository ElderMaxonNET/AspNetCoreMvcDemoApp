using SadLib.Data;
using SadLib.Data.Abstractions;

namespace AspNetCoreMvcDemoApp.Models.Abstractions
{
    public abstract class BaseSearchDto : ISqlSortOptions, ISqlPageOptions
    {
        public virtual int Page { get; set; } = 1;
        public virtual int PageSize { get; set; } = 10;
        public virtual string SortColumn { get; set; } = "Id";
        public virtual string SortDirection { get; set; } = "desc";

        public bool Enable { get; set; }
    }
}
