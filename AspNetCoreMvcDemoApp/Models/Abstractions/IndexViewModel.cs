namespace AspNetCoreMvcDemoApp.Models.Abstractions
{
    public class IndexViewModel<TListDto, TSearchDto>(TableViewContext<TListDto, TSearchDto> context) : BaseIndexViewModel<TListDto, TSearchDto>(context)
        where TListDto : class, new()
        where TSearchDto : BaseSearchDto, new();
}
