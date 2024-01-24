using System.Net;
using MoviesBE.DTOs;
using MoviesBE.Exceptions;
using Neo4j.Driver;

namespace MoviesBE.Middleware;

public class ExceptionMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError("An error occurred: {ExceptionType} - {Message} - Request: {Method} {Url}",
                ex.GetType().Name, ex.Message, context.Request.Method, context.Request.Path);

            _logger.LogDebug(ex, "Full exception details");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case KeyNotFoundException keyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = keyNotFoundException.Message
                }.ToString());

            case DatabaseAccessException databaseAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = databaseAccessException.Message
                }.ToString());

            case ExternalApiException externalApiException:
                context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                return context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = externalApiException.Message
                }.ToString());

            case DataFormatException dataFormatException:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                return context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = dataFormatException.Message
                }.ToString());

            case ExternalServiceException externalServiceException:
                context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                return context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = externalServiceException.Message
                }.ToString());

            case Neo4jException:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "A database error occurred."
                }.ToString());


            // TODO: handle other exception types ...

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Internal Server Error."
                }.ToString());
        }
    }
}