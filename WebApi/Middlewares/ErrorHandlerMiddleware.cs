using Application.Exceptions;
using Application.Wrappers;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApi.Controllers;

namespace WebApi.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
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
                var response = context.Response;
                response.ContentType = "application/json";
                var responseModel = new ApiResponse<string> { Succecced = false, Message = ex.Message };

                switch (ex)
                {
                    case ApiException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case ValidationErrorException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseModel.Errors = e.Errors;
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                _logger.LogError(ex, ex.Message);
                var result = JsonSerializer.Serialize(responseModel);
                await response.WriteAsync(result);
            }

        }
    }
}
