using EventApp.Core.Exceptions;
using EventApp.Models.SharedDTO;
using System.Net;
using System.Text.Json;

namespace EventApp.Api.Middleware {

    public class ExceptionHandlerMiddleware {

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger) {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) {

            try {

                await _next(context);

            } catch (Exception ex) {

                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

                await HandleException(context, ex);
            }

        }

        private static Task HandleException(HttpContext context, Exception exception) {
            
            HttpStatusCode statusCode;
            object responsePayload;

            switch (exception) {

                case DuplicateResourceException duplicateResourceException:
                    statusCode = HttpStatusCode.Conflict;
                    responsePayload = new ErrorResponse((int)statusCode, "Конфликт ресурса", duplicateResourceException.Message);
                    break;

                case InvalidCredentialsException invalidCredentialsException:
                    statusCode = HttpStatusCode.Unauthorized;
                    responsePayload = new ErrorResponse(statusCode, "HTTP Error 401 – Ошибка авторизации.");
                    break;

                default: 
                    statusCode = HttpStatusCode.InternalServerError; 
                    responsePayload = new ErrorResponse(statusCode, "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже.");
                    break;

            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var jsonResponse = JsonSerializer.Serialize(responsePayload, new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            return context.Response.WriteAsync(jsonResponse);

        }

    }

}
