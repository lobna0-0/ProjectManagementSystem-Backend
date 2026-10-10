using System.Net;
using System.Text.Json;
using ProjectManagementSystem.DTOs.Common;
using ProjectManagementSystem.Exceptions;

namespace ProjectManagementSystem.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<GlobalExceptionMiddleware>
        _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred.");

            await HandleExceptionAsync(
                context,
                ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType =
            "application/json";

        var statusCode = exception switch
        {
            NotFoundException =>
                StatusCodes.Status404NotFound,

            BadRequestException =>
                StatusCodes.Status400BadRequest,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode =
            statusCode;

        var environment =
            context.RequestServices
                .GetRequiredService<
                    IWebHostEnvironment>();

        var response = new ErrorResponseDto
        {
            StatusCode = statusCode,

            Message = exception switch
            {
                NotFoundException =>
                    exception.Message,

                BadRequestException =>
                    exception.Message,

                _ =>
                    "An unexpected error occurred."
            },

            Details = environment.IsDevelopment()
                ? exception.StackTrace
                : null
        };

        var json =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}