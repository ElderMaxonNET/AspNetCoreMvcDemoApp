using AspNetCoreMvcDemoApp.Controllers.Abstractions;
using AspNetCoreMvcDemoApp.Core.Web.Attributes;
using AspNetCoreMvcDemoApp.Core.Web.Common.Services;
using Microsoft.AspNetCore.Mvc;
using SadLib.Extensions;
using SadLib.Web.Upload.Abstractions;
using SadLib.Web.Upload.Models;

namespace AspNetCoreMvcDemoApp.Controllers
{
    [UserAuthorization(RoleTypes.Uploader)]
    public class UserFilesController(ICommonServices commonServices) : BaseFilesController(commonServices)
    {
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromServices] IUploadService upload, UserFilesCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Fail();
            }

            // Prepare multiple file upload context
            var uploadContext = upload.CreateContext()
                .AddFiles(dto.Items,
                    x => x.File,
                    x => x.Description.ToSlug(),
                    x => x.Description
                )
                .AddOptions(new UploadOptions(Provider: CurrentStorageProvider, Overwrite: false))
                .Build();

            // Execute the upload and handle the result
            var result = await upload.SaveAndExecuteAsync(
                uploadContext,
                onSuccess: async (files) =>
                {
                    await Data.UserFiles.SaveAsync(ToModels(files, UserContext.Current.Id));

                    return Success();
                },
                onFail: (err) => Fail(err)
            );

            return result;
        }

        public async Task<IActionResult> Edit(int id)
        {
            UserFiles? model = await Data.UserFiles.GetAsync(id);
            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var dto = new UserFilesUpdateDto();
            Mapper.ToDto(model, dto);

            return View(dto);
        }


        [HttpPost]
        public async Task<IActionResult> Edit([FromServices] IUploadService upload, UserFilesUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Fail();
            }

            // Retrieve the existing model from the database
            var model = await Data.UserFiles.GetAsync(dto.Id);
            if (model == null)
            {
                return NotFound();
            }

            // Dto to entity mapping
            Mapper.ToEntity(dto, model);

            // Prepare file upload context
            var uploadContext = upload.CreateContext()
                .AddFile(dto.File, fileName: dto.Description.ToSlug(), description: dto.Description)
                .AddOptions(new UploadOptions(Provider: CurrentStorageProvider, Overwrite: true, ExistingFileName: model.Name))
                .Build();

            // Execute upload and handle the result
            var result = await upload.SaveAndExecuteAsync(
                uploadContext,
                onSuccess: async (files) =>
                {
                    if (files.Count > 0)
                    {
                        // If a new file was uploaded, update the model
                        MapToFileModel(files.First(), model);
                    }
                    else
                    {
                        // If no new file was uploaded, we can optionally rename the existing file based on the new description
                        model.Name = CurrentStorageProvider.Rename(oldFileName: model.Name, newFileName: model.Description.ToSlug());
                    }

                    await Data.UserFiles.SaveAsync([model]);

                    return Success();
                },
                onFail: (err) => Fail(err)
            );

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] UserFilesDeleteDto model)
        {
            if (!ModelState.IsValid)
            {
                return Fail();
            }

            var fileNames = await Data.UserFiles.GetFileNamesAsync(model.Ids, UserContext.RestrictionId);

            if (fileNames.Any())
            {
                CurrentStorageProvider.Delete(fileNames);
                await Data.UserFiles.DeleteAsync(model.Ids, UserContext.RestrictionId);
            }

            var result = await GetPagedResults(model.SearchDto);

            return Success(result);
        }
    }
}
