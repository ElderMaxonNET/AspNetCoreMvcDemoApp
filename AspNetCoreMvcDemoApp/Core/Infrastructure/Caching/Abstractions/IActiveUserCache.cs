namespace AspNetCoreMvcDemoApp.Core.Infrastructure.Caching.Abstractions
{
    public interface IActiveUserCache
    {
        bool IsActive(int userId);
        void Update(IEnumerable<int> activeIds);
    }
}
