using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using AspNetCoreMvcDemoApp.Core.Web.Common.Services;
using Microsoft.AspNetCore.Mvc;
using SadLib.Data.Mapping.Abstractions;
using SadLib.Infrastructure.Storage.Abstractions;

namespace AspNetCoreMvcDemoApp.Controllers.Abstractions
{
    public abstract class BaseController(ICommonServices commonServices) : Controller
    {
        protected readonly IMapperService Mapper = commonServices.Mapper;
        protected readonly IStorageService Storage = commonServices.Storage;
        protected readonly IDataService Data = commonServices.Data;
        protected readonly IUserContext UserContext = commonServices.User;

        private ObjectResult CreateProblem(string detail, string title, Dictionary<string, object?>? extensions = null)
        {
            return Problem(
                detail: detail,
                statusCode: StatusCodes.Status400BadRequest,
                title: title,
                instance: $"{Request.Method} {Request.Path}",
                type: this.GetType().Name,
                extensions: extensions
            );
        }

        protected ObjectResult Fail(string msg, string title = "Operation failed.")
        {
            return CreateProblem(msg, title);
        }

        protected ObjectResult DataNotFound()
        {
            return Fail("Requested data could not be found.", "Not Found.");
        }

        protected ObjectResult Fail()
        {
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrEmpty(msg));

            return CreateProblem("One or more validation errors occurred.", "Validation Failed.", new Dictionary<string, object?> { ["validationErrors"] = errorMessages });
        }

        protected ObjectResult Success(string msg = "Operation completed successfully.")
        {
            return Ok(new { Detail = msg });
        }

        protected ObjectResult Success(object data)
        {
            return Ok(data);
        }

    }
}
