using Contracts;
using Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Presentation.Controllers
{
    [Route("api/[controller]/[action]")]
    public abstract class BaseController : ControllerBase
    {
        protected BaseController()
        {
        }

        #region Return Methods
        protected PagedResponse<T> GetSearchResult<T>(HttpRequest request, IQueryable queryable) where T : class
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddEntityType(typeof(T));
            var edmModel = modelBuilder.GetEdmModel();

            var queryOptions = new ODataQueryOptions<T>(new ODataQueryContext(edmModel, typeof(T), new ODataPath()), request);

            ODataQuerySettings settings = new()
            {
                PageSize = queryOptions.Top == null ? ApiConfiguration.DataQueryDefaultPageSize : queryOptions.Top.Value
            };

            IQueryable filteredList = queryOptions.ApplyTo(queryable, settings);

            Uri? nextPageLink = queryOptions.Request.ODataFeature().NextLink;

            var skip = queryOptions.Skip?.Value;
            skip ??= 0;

            if (skip + settings.PageSize < queryOptions.Request.ODataFeature().TotalCount)
                nextPageLink = Request.GetNextPageLink(settings.PageSize.Value, null, null);

            return new PagedResponse<T>(filteredList as IEnumerable<T>, nextPageLink, queryOptions.Request.ODataFeature().TotalCount);
        }

        protected IActionResult ReturnBadRequest(ILogger _logger, Exception e)
        {
            _logger.LogError("Get Property Failed " + e);
            return BadRequest(e.Message);
        }

        protected IActionResult ReturnBadRequest(IdentityResult result)
        {
            var errors = result.Errors.Select(it => it.Description);
            var response = new
            {
                Message = string.Join(",", errors),
                Status = 500,
            };
            return BadRequest(response);
        }

        protected IActionResult EntityNotFound(ILogger logger, string model, string message)
        {
            var response = new ErrorResponse
            {
                Message = message,
                Code = StatusCodes.Status404NotFound,
            };

            logger.LogError($"Ëntity not found. Request: {model}. Errors: \"{response.Message}\"");

            return NotFound(response);
        }

        protected IActionResult ReturnInvalidRequest(ModelStateDictionary modelState, ILogger logger, string model)
        {
            var errors = modelState.Values.SelectMany(it => it.Errors).Select(it => it.ErrorMessage);

            var response = new ErrorResponse
            {
                Message = "Invalid model request",
                Code = StatusCodes.Status400BadRequest,
                Errors = errors.ToList()
            };

            logger.LogError($"Invalid model request.Request: {model}. Errors: \"{response.Message}\"");

            return BadRequest(response);
        }

        protected void LogInfoAction(ILogger logger, string actionMmessage)
        {
            logger.LogInformation("{Message}{UserName}", actionMmessage, User.Identity?.Name);
        }
        #endregion
    }
}
