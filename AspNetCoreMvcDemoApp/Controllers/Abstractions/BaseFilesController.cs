using AspNetCoreMvcDemoApp.Core.Web.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SadLib.Data.Providers;
using SadLib.Infrastructure.Storage.Abstractions;
using SadLib.Web.Upload;
using SadLib.Web.Upload.Models;

namespace AspNetCoreMvcDemoApp.Controllers.Abstractions
{
    public abstract class BaseFilesController(ICommonServices commonServices) : BaseController(commonServices)
    {
        protected IStorageProvider CurrentStorageProvider => field ?? Storage["/uploads/files"];

        protected static List<SelectListItem> GetFastTimes()
        {
            return
            [
            new() { Value = "1", Text = "Today" },
            new() { Value = "2", Text = "This Week" },
            new() { Value = "3", Text = "This Month" },
            new() { Value = "4", Text = "This Year" }
            ];
        }

        protected static List<SelectListItem> GetFileSizeOperators()
        {
            return
            [
                new() { Value = "1", Text = "(<) Less Than" },
                new() { Value = "2", Text = "(=)Equal" },
                new() { Value = "3", Text = "(>) Greater Than" }
            ];
        }

        protected static void MapToFileModel(UploadedFile file, UserFiles model)
        {
            string[] units = ["Byte", "KB", "MB", "GB", "TB"];
            double size = file.Size;
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            model.Name = file.Name;
            model.Description = file.Description;
            model.ContentType = file.ContentType;
            model.SizeInBytes = file.Size;
            model.FormattedSize = (decimal)Math.Round(size, 2);
            model.SizeUnit = units[unitIndex];
        }

        protected static IEnumerable<UserFiles> ToModels(List<UploadedFile> items, int uploaderId)
        {
            return items.Select(file =>
            {
                var model = new UserFiles { UploaderId = uploaderId };
                MapToFileModel(file, model);
                return model;
            });
        }


        protected static List<SelectListItem> GetContentTypes()
        {
            return [.. FileTypeRegistry.GetMimeTypes().Select(x => new SelectListItem(x, x))];
        }

        protected Task<PagedResult<UserFilesListDto>> GetPagedResults(UserFilesSearchDto searchDto)
        {
            if (UserContext.Role == RoleTypes.Uploader)
            {
                searchDto.UploaderId = UserContext.Current.Id;
            }
            return Data.UserFiles.GetPagedResultAsync(searchDto);
        }

        public async Task<IActionResult> Index(bool search)
        {
            var searchUrl = Url.Action(nameof(Search))!;
            var searchDto = new UserFilesSearchDto() { Enable = search };
            var pagedResult = await GetPagedResults(searchDto);

            var context = TableViewContext.Create(pagedResult, searchDto, searchUrl)
                .AddLookup(nameof(UserFilesSearchDto.FastTimeType), GetFastTimes())
                .AddLookup(nameof(UserFilesSearchDto.FileSizeOperator), GetFileSizeOperators())
                .AddLookup(nameof(UserFilesSearchDto.ContentType), GetContentTypes())
                .AddMetadataTo(ViewData);

            return View(context.ToModel());
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] UserFilesSearchDto searchDto)
        {
            if (!ModelState.IsValid)
            {
                return Fail();
            }

            var result = await GetPagedResults(searchDto);

            return Success(result);
        }

    }
}
