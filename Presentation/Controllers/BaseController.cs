using Contracts;
using Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public abstract class BaseController : ControllerBase
    {
        protected BaseController()
        {
        }

        #region Return Methods
        protected PagedResponse<T> GetSearchResult<T>(HttpRequest request, IQueryable queryable) where T : class
        {
            ODataConventionModelBuilder modelBuilder = new();
            modelBuilder.AddEntityType(typeof(T));
            Microsoft.OData.Edm.IEdmModel edmModel = modelBuilder.GetEdmModel();

            ODataQueryOptions<T> queryOptions = new(new ODataQueryContext(edmModel, typeof(T), new ODataPath()), request);

            ODataQuerySettings settings = new()
            {
                PageSize = queryOptions.Top == null ? ApiConfiguration.DataQueryDefaultPageSize : queryOptions.Top.Value
            };

            IQueryable filteredList = queryOptions.ApplyTo(queryable, settings);

            Uri? nextPageLink = queryOptions.Request.ODataFeature().NextLink;

            int? skip = queryOptions.Skip?.Value;
            skip ??= 0;

            if (skip + settings.PageSize < queryOptions.Request.ODataFeature().TotalCount)
                nextPageLink = Request.GetNextPageLink(settings.PageSize.Value, null, null);

            return new PagedResponse<T>(filteredList as IEnumerable<T>, nextPageLink, queryOptions.Request.ODataFeature().TotalCount);
        }

        protected IActionResult ReturnBadRequest(ILogger logger, string model, string message, List<string> errors)
        {
            ErrorResponse response = new ErrorResponse()
            {
                Message = message,
                Code = StatusCodes.Status400BadRequest,
                Errors = errors
            };

            logger.LogError($"Bad request. Request: {model}. Errors: \"{response.Message}\"");

            return BadRequest(response);
        }

        protected IActionResult ReturnEntityNotFound(ILogger logger, string model, string message, List<string> errors)
        {
            ErrorResponse response = new ErrorResponse
            {
                Message = message,
                Code = StatusCodes.Status404NotFound,
                Errors = errors
            };

            logger.LogError($"Entity not found. Request: {model}. Errors: \"{response.Message}\"");

            return NotFound(response);
        }

        protected IActionResult ReturnInvalidRequest(ModelStateDictionary modelState, ILogger logger, string model)
        {
            IEnumerable<string> errors = modelState.Values.SelectMany(it => it.Errors).Select(it => it.ErrorMessage);

            ErrorResponse response = new ErrorResponse
            {
                Message = "Invalid request",
                Code = StatusCodes.Status400BadRequest,
                Errors = errors.ToList()
            };

            logger.LogError($"Invalid model request. Request: {model}. Errors: \"{response.Message}\"");

            return BadRequest(response);
        }

        protected void LogInfoAction(ILogger logger, string actionMmessage)
        {
            logger.LogInformation("{Message}{UserName}", actionMmessage, User.Identity?.Name);
        }
        #endregion
    }
}
