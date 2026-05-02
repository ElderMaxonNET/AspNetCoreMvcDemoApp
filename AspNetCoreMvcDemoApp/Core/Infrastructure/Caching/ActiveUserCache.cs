using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using AspNetCoreMvcDemoApp.Core.Infrastructure.Caching.Abstractions;
using System.Collections.Frozen;

namespace AspNetCoreMvcDemoApp.Core.Infrastructure.Caching
{
    public class ActiveUserCache : IActiveUserCache
    {
        private FrozenSet<int> _activeUserIds = [];

        public ActiveUserCache(IDataService dataService)
        {
            dataService.Users.OnActiveUsersChanged += Update;
        }

        private static int GetSafeId(int userId) {
            return userId < 1 ? throw new ArgumentOutOfRangeException(nameof(userId)) : userId;
        }

        public void Update(IEnumerable<int> activeIds) =>
            _activeUserIds = activeIds.Select(GetSafeId).ToFrozenSet();

        public bool IsActive(int userId) => _activeUserIds.Contains(userId);
    }
}
