using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SadLib.Data;
using SadLib.Data.Abstractions;

namespace AspNetCoreMvcDemoApp.Models.Abstractions
{
    public interface ITableMetadata
    {
        IEnumerable<T> GetLookups<T>(string key);
        T? GetLookup<T>(string key) where T : class;
    }

    public class TableViewContext<TListDto, TSearchDto>(IPagedResult<TListDto> pagedResult, TSearchDto searchDto, string loadUrl) : ITableMetadata
        where TListDto : class, new()
        where TSearchDto : BaseSearchDto, new()
    {
        private readonly Dictionary<string, object> Lookups = [];

        public IPagedResult<TListDto> Data { get; } = pagedResult;
        public TSearchDto Search { get; } = searchDto;
        public string LoadUrl { get; } = loadUrl;

        public TableViewContext<TListDto, TSearchDto> AddLookup(string key, object value)
        {
            Lookups[key] = value;
            return this;
        }

        public IEnumerable<T> GetLookups<T>(string key)
        {
            if (Lookups.TryGetValue(key, out var value) && value is IEnumerable<T> list)
            {
                return list;
            }
            return [];
        }

        public T? GetLookup<T>(string key) where T : class
        {
            if (Lookups.TryGetValue(key, out var value) && value is T item)
            {
                return item;
            }
            return null;
        }

        public TableViewContext<TListDto, TSearchDto> AddMetadataTo(ViewDataDictionary viewData)
        {
            viewData[TableViewContext.MetadataKey] = (ITableMetadata)this;
            return this;
        }

        public IndexViewModel<TListDto, TSearchDto> ToModel()
        {
            return new IndexViewModel<TListDto, TSearchDto>(this);
        }
    }

    public static class TableViewContext
    {
        public const string MetadataKey = "TableMetadata";

        public static TableViewContext<TList, TSearch> Create<TList, TSearch>(
            IPagedResult<TList> result, TSearch dto, string url)
            where TList : class, new()
            where TSearch : BaseSearchDto, new()
            => new(result, dto, url);
    }
}
