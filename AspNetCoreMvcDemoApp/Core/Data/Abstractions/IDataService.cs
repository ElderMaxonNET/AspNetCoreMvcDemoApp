namespace AspNetCoreMvcDemoApp.Core.Data.Abstractions
{
    public interface IDataService
    {
        public UserRepository Users { get; }

        public UserRolesRepository UserRoles { get; }

        public UserFilesRepository UserFiles { get; }
    }
}
