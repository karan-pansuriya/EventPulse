using System.Net;
using System.Text.Json;
using EventPulse.BLL.Models.Request;
using EventPulse.BLL.Exceptions;

namespace EventPulse.API.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message = ex.Message;

        switch (ex)
        {
            case NotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                break;

            case BadRequestException:
                statusCode = (int)HttpStatusCode.BadRequest;
                break;

            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = ApiMessages.InternalServerError;
                break;
        }

        context.Response.StatusCode = statusCode;

        ApiResponse<object> response = new ApiResponse<object>(
            false,
            statusCode,
            message,
            null
        );

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
