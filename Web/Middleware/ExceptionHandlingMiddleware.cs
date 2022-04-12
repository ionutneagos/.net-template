using Contracts;
using Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Web.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
                if (context.Response.StatusCode == 401)
                    throw new UnauthorizedAccessException("Token has expired");
            }
            catch (Exception e)
            {
                _logger.LogError(e, message: e.Message);

                await HandleExceptionAsync(context, e);
            }
        }
        private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType = "application/json";
            var exceptionType = exception.GetType();

            httpContext.Response.StatusCode = exception switch
            {
                var _ when exceptionType == typeof(BadRequestException) => StatusCodes.Status400BadRequest,
                var _ when exceptionType == typeof(UnauthorizedAccessException) => StatusCodes.Status401Unauthorized,
                var _ when exceptionType == typeof(NotFoundException) => StatusCodes.Status404NotFound,
                var _ when exceptionType == typeof(ValidationException) => StatusCodes.Status422UnprocessableEntity,
                AppException e when exceptionType == typeof(AppException) => e.Code,
                _ => StatusCodes.Status500InternalServerError,
            };
            var response = new ErrorResponse
            {
                Code = httpContext.Response.StatusCode,
                Message = exception.Message
            };

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}