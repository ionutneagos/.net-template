using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Presentation.Controllers
{
    [Route("api/[controller]/[action]")]
    public abstract class BaseController : ControllerBase
    {
        protected BaseController()
        {
        }

        #region Return Methods
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
            var response = new
            {
                Message = message,
                Status = 500,
            };

            logger.LogError($"Invalid model request. Request: {model}. Errors: \"{response.Message}\"");

            return BadRequest(response);
        }

        protected IActionResult ReturnInvalidRequest(ModelStateDictionary modelState, ILogger logger, string model)
        {
            var errors = modelState.Values.SelectMany(it => it.Errors).Select(it => it.ErrorMessage);
            var response = new
            {
                Message = string.Join(",", errors),
                Status = 500,
            };

            logger.LogError($"Invalid model request.Request: {model}. Errors: \"{response.Message}\"");

            return BadRequest(response);
        }

        protected IActionResult ReturnNotAllowedRequest(ILogger logger, string actionMmessage)
        {
            logger.LogInformation("{Message}{UserName}", actionMmessage, User.Identity?.Name);
            var response = new
            {
                Message = actionMmessage,
                Status = 500,
            };

            return BadRequest(response);
        }

        protected void LogInfoAction(ILogger logger, string actionMmessage)
        {
            logger.LogInformation("{Message}{UserName}", actionMmessage, User.Identity?.Name);
        }
        #endregion
    }
}
