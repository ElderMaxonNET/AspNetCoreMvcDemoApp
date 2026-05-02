using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using SadLib.Data.Mapping.Abstractions;
using SadLib.Infrastructure.Storage.Abstractions;

namespace AspNetCoreMvcDemoApp.Core.Web.Common.Services
{
    public class CommonServices(IMapperService mapper, IStorageService storage, IDataService data, IUserContext user) : ICommonServices
    {
        public IMapperService Mapper => mapper;
        public IStorageService Storage => storage;
        public IDataService Data => data;
        public IUserContext User => user;
    }
}
