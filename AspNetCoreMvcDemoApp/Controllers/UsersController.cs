using AspNetCoreMvcDemoApp.Controllers.Abstractions;
using AspNetCoreMvcDemoApp.Core.Web.Attributes;
using AspNetCoreMvcDemoApp.Core.Web.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SadLib.Data.Providers;
using SadLib.Infrastructure.Storage.Abstractions;
using SadLib.Infrastructure.Storage.Extensions;
using SadLib.Web.Upload.Abstractions;
using SadLib.Web.Upload.Models;

namespace AspNetCoreMvcDemoApp.Controllers
{
    [UserAuthorization]
    public class UsersController(ICommonServices commonServices) : BaseController(commonServices)
    {
        private const string AvatarFolderPathSmall = "/uploads/avatar/small";
        private const string AvatarFolderPathMedium = "/uploads/avatar/medium";
        private const string AvatarFolderPathLarge = "/uploads/avatar/large";
        private const string FilesFolderPath = "/uploads/files";

        private static List<SelectListItem> GetFastTimes()
        {
            return
            [
            new() { Value = "1", Text = "Today" },
            new() { Value = "2", Text = "This Week" },
            new() { Value = "3", Text = "This Month" },
            new() { Value = "4", Text = "This Year" }
            ];
        }

        private async Task<IEnumerable<UserRole>> GetRolesAsync()
        {
            return await Data.UserRoles.GetAllAsync();
        }

        private UploadContext GetUploadContext(
            IUploadService uploadService,
            IFormFile? postedFile,
            string? existingFileName = null)
        {
            return uploadService
                .CreateContext()
                .AddFile(postedFile)
                .AddOptions(GetUploadOptions(existingFileName))
                .Build();
        }

        private UploadOptions[] GetUploadOptions(string? existingFileName = null)
        {
            UploadOptions CreateOption(IStorageProvider provider, int dimension, int quality) => new(
                Provider: provider,
                Overwrite: false,
                ExistingFileName: existingFileName,
                Processor: new ImageProcessor
                (
                    width: dimension,
                    height: dimension,
                    mode: SixLabors.ImageSharp.Processing.ResizeMode.Pad,
                    convertToWebp: true,
                    quality: quality
                ));

            return
            [
                CreateOption(Storage[AvatarFolderPathSmall], 150, 70),
                CreateOption(Storage[AvatarFolderPathMedium], 300, 80),
                CreateOption(Storage[AvatarFolderPathLarge], 500, 90)
            ];
        }

        private Task<PagedResult<UserListDto>> GetPagedResults(UserSearchDto searchDto)
        {
            return Data.Users.GetPagedResultAsync(searchDto);
        }

        public async Task<IActionResult> Index(bool search)
        {
            var searchUrl = Url.Action(nameof(Search))!;
            var searchDto = new UserSearchDto() { Enable = search };
            var roles = await GetRolesAsync();
            var pagedResult = await GetPagedResults(searchDto);

            var context = TableViewContext.Create(pagedResult, searchDto, searchUrl)
                .AddLookup("Roles", roles)
                .AddLookup(nameof(UserSearchDto.FastTimeType), GetFastTimes())
                .AddMetadataTo(ViewData);

            return View(context.ToModel());
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] UserSearchDto searchDto)
        {
            if (!ModelState.IsValid)
            {
                return Fail();
            }

            var result = await GetPagedResults(searchDto);
            return Success(result);
        }

        public async Task<IActionResult> Create()
        {
            var result = new UserCreateDto
            {
                Active = true,
                RoleList = await GetRolesAsync()
            };
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromServices] IUploadService upload, UserCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Fail();
            }

            // Map DTO to entity
            var model = new User();
            Mapper.ToEntity(dto, model);

            // Hash the password and set the hash and salt in the model
            model.Secure(dto.Password);

            // Prepare file upload context
            var uploadContext = GetUploadContext(upload, dto.File);

            // Execute upload and handle the result
            var result = await upload.SaveAndExecuteAsync(
                uploadContext,
                onSuccess: async (files) =>
                {
                    // In create action, we expect a file to be uploaded, so we can directly set the avatar filename without checking for null
                    model.Avatar = files.First().Name;

                    // Insert the new user into the database and get the generated ID
                    await Data.Users.SaveAsync(model);

                    return Success();
                },
                onFail: (err) => Fail(err)
            );

            return result;
        }

        public async Task<IActionResult> Edit(int id)
        {
            User? model = await Data.Users.GetAsync(id);
            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var result = new UserUpdateDto
            {
                RoleList = await GetRolesAsync()
            };

            Mapper.ToDto(model, result);

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromServices] IUploadService upload, UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Fail();
            }

            // Retrieve the existing user from the database
            var model = await Data.Users.GetAsync(dto.Id);
            if (model == null)
            {
                return DataNotFound();
            }

            // Map the updated fields from the DTO to the existing model
            Mapper.ToEntity(dto, model);

            // Hash the new password if provided and update the model
            model.Secure(dto.Password);

            // Prepare file upload context
            var uploadContext = GetUploadContext(upload, dto.File, model.Avatar);

            // Execute upload and handle the result
            var result = await upload.SaveAndExecuteAsync(
                uploadContext,
                onSuccess: async (files) =>
                {
                    // If a new file was uploaded, update the avatar filename in the model
                    var newFile = files.FirstOrDefault();
                    if (newFile != null)
                    {
                        model.Avatar = newFile.Name;
                    }

                    await Data.Users.SaveAsync(model);

                    return Success();
                },
                onFail: (err) => Fail(err)
            );

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] UserDeleteDto model)
        {
            if (!ModelState.IsValid)
            {
                return Fail();
            }

            // Get all file names associated with the users to be deleted
            var fileNames = await Data.Users.GetAllFileNamesAsync(model.Ids);

            // Delete the files from all relevant storage providers
            Storage
                .GetProviders(AvatarFolderPathSmall, AvatarFolderPathMedium, AvatarFolderPathLarge, FilesFolderPath)
                .DeleteFiles(fileNames);

            // Delete the user records from the database
            await Data.Users.DeleteAsync(model.Ids);

            // Return filtered results after deletion
            var result = await GetPagedResults(model.SearchDto);
            return Success(result);
        }

    }
}
