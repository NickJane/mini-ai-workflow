using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nop.WebApiFramework.Exceptions;
using Nop.WebApiFramework.OpenTelemetry;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace Nop.WebApiFramework.Pipeline;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly static HttpClient _botClient = new();
    private readonly string _appName;
    private readonly IConfiguration _configuration;
    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        string appName,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _appName = appName;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            // 你可以在这里进行相关的日志记录
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int httpStatus = exception switch
        {
            ApplicationException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        if (exception is WebApiException e)
        {
            var response = GetResponse(context, e.ErrorCode, e.ErrorMessage, e.ResponseData);

            _logger.LogDebug($"{e.ErrorCode}|{e.ErrorMessage}");

            await WriteResponse(e.HttpStatus, response, context);
        }
        else
        {
            _logger.LogError(exception, exception.Message);

            var message = exception.Message;

            if (_configuration.GetValue<bool>("SystemExtensionShield", true))
            {
                message = "系统内部错误";
            }

            await WriteResponse(httpStatus, new JsonResponse(500, message, context.GetRequestId()), context);
        }
    }


    private object GetResponse(HttpContext context, int errorCode, string errorMessage, object? data)
    {
        if (data == null)
        {
            return new JsonResponse(errorCode, errorMessage, context.GetRequestId());
        }

        return new JsonResponse<object>(data, errorCode, errorMessage, context.GetRequestId());
    }

    /// <summary>
    /// 写入响应体
    /// </summary>
    /// <param name="httpStatus"></param>
    /// <param name="response"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task WriteResponse(int httpStatus, object response, HttpContext context)
    {
        context.Response.ContentType = "application/json;utf-8";
        context.Response.StatusCode = httpStatus;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(response), Encoding.UTF8);
    }

}