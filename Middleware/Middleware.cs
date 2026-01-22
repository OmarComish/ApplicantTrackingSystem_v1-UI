using System.Net;
using System.Text.Json;

namespace ATS.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ErrorResponse
            {
                Message = exception.Message,
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            switch (exception)
            {
                case ArgumentNullException _:
                case ArgumentException _:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                
                case KeyNotFoundException _:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "The requested resource was not found.";
                    break;
                
                case UnauthorizedAccessException _:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access.";
                    break;
                
                case InvalidOperationException _:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;
                
                default:
                    response.Message = "An internal server error occurred. Please try again later.";
                    break;
            }

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
