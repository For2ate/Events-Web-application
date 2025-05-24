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

                case NotFoundException notFoundException: 
                    statusCode = HttpStatusCode.NotFound;
                    responsePayload = new ErrorResponse((int)statusCode, "Ресурс не найден", exception.Message);
                    break;

                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    responsePayload = new ErrorResponse((int)statusCode, "Некорректный запрос", badRequestException.Message);
                    break;

                case FluentValidation.ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    var errors = string.Join("; ", validationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                    responsePayload = new ErrorResponse((int)statusCode, "Ошибка валидации", errors);
                    break;

                case ConflictException conflictException: 
                    statusCode = HttpStatusCode.Conflict;
                    responsePayload = new ErrorResponse((int)statusCode, "Конфликт операции", conflictException.Message);
                    break;

                case UnauthorizedAccessException unauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized;
                    responsePayload = new ErrorResponse((int)statusCode, "Доступ запрещен", unauthorizedException.Message);
                    break;

                case OperationFailedException operationFailedException:
                    statusCode = HttpStatusCode.InternalServerError;
                    responsePayload = new ErrorResponse((int)statusCode, "Ошибка выполнения операции", "Произошла ошибка при обработке вашего запроса. Пожалуйста, попробуйте позже.");         
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    responsePayload = new ErrorResponse((int)statusCode, "Произошла внутренняя ошибка сервера.", "Пожалуйста, попробуйте позже.");
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
