using AspNetCoreMvcDemoApp.Controllers.Abstractions;
using AspNetCoreMvcDemoApp.Core.Web.Attributes;
using AspNetCoreMvcDemoApp.Core.Web.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreMvcDemoApp.Controllers
{
    [UserAuthorization(RoleTypes.Downloader)]
    public class DownloadController(ICommonServices commonServices) : BaseFilesController(commonServices)
    {
        public async Task<IActionResult> Get(int id)
        {
            var fileEntry = await Data.UserFiles.GetAsync(id);
            if (fileEntry == null)
            {
                return DataNotFound();
            }

            var stream = CurrentStorageProvider.OpenRead(fileEntry.Name);
            return File(stream, fileEntry.ContentType, fileEntry.Name);
        }
    }
}
