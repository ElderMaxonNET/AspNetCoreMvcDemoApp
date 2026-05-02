using SadLib.Data;

namespace AspNetCoreMvcDemoApp.Core.Data
{
    using Abstractions;
    using SadLib.Data.Abstractions;

    public class DataService(IDbClient db) : IDataService
    {
        public UserRepository Users => field ??= new UserRepository(db);
        public UserRolesRepository UserRoles => field ??= new UserRolesRepository(db);
        public UserFilesRepository UserFiles => field ??= new UserFilesRepository(db);
    }
}
