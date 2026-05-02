using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using Dapper;
using SadLib.Data;
using SadLib.Data.Abstractions;
using System.Data;

namespace AspNetCoreMvcDemoApp.Core.Data.Repositories
{
    public class UserRolesRepository(IDbClient dbClient) : BaseRepository<UserRole>(dbClient)
    {
        public Task<IEnumerable<UserRole>> GetAllAsync() =>
            QueryListAsync<UserRole>("SELECT * FROM dbo.UserRoles ORDER BY Name");

        public Task<UserRole?> GetAsync(int id) =>
            QuerySingleAsync<UserRole>("SELECT * FROM dbo.UserRoles WHERE Id = @Id", new { Id = id });
    }
}
