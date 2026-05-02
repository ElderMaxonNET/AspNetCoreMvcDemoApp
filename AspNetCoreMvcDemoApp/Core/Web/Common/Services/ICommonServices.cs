using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using SadLib.Data.Mapping.Abstractions;
using SadLib.Infrastructure.Storage.Abstractions;

namespace AspNetCoreMvcDemoApp.Core.Web.Common.Services
{
    public interface ICommonServices
    {
        IMapperService Mapper { get; }
        IStorageService Storage { get; }
        IDataService Data { get; }
        IUserContext User { get; }
    }
}
