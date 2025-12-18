using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Nop.Infrastructure;
using Nop.WebApiFramework.OpenTelemetry;
using Microsoft.Extensions.Logging;

namespace Nop.WebApiFramework.Pipeline
{
    /// <summary>
    /// 1. 设置Body内容可以重复读取, 可通过context.Items["bodyString"], 或者 context.GetFeatureBody()获取
    /// 2. 将请求体和响应体写入seq, 方便排查问题
    /// </summary>
    public class HttpRequestBodyMiddleware
    {
        private const string JSON_CONTENT_TYPE = "application/json";
        /// <summary>
        /// 最大Body大小, 设置为100KB   
        /// </summary> 
        private const int MAX_BODY_SIZE = 10 * 1024;
        private readonly RequestDelegate _next;
        public HttpRequestBodyMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var _logger = context.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<HttpRequestBodyMiddleware>>();

            string curl = context.Request.ToCurl();
            if (IsJsonRequest(context.Request))
            {
                string bodyString = await context.GetRequestBodyAsync();
                context.Items["bodyString"] = bodyString;
                if (!string.IsNullOrEmpty(bodyString))
                {
                    curl += $" -d '{bodyString}'";
                }
            }
            _logger?.LogInformation($"请求信息: {curl}");

            // 如果是 SSE 响应，直接放行，不替换响应流, 否则流式响应会卡住.
            if (context.Request.Path.HasValue && 
            (context.Request.Path.Value.Contains("sse", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.Value.Contains("chat-messages", StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }



            // 替换响应流以便能够读取响应内容
            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // 继续处理请求
            await _next(context);

            // 检查是否为文件流响应
            if (IsFileStreamResponse(context.Response))
            {
                // 如果是文件流响应，直接写回原始流
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
                return;
            }

            // 读取响应内容(如果是JSON响应)
            if (IsJsonResponse(context.Response))
            {
                memoryStream.Position = 0;
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
                if (responseBody?.Length <= MAX_BODY_SIZE)
                {
                    _logger.LogInformation($"响应体: {responseBody}");
                }
                else
                {
                    _logger.LogWarning($"响应体过大: {responseBody?.Length ?? 0} 字节; 部分响应体: {responseBody?.Substring(0, MAX_BODY_SIZE)}");
                }
            }

            // 将响应内容写回原始流
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }

        private bool IsJsonRequest(HttpRequest request)
        {
            return request.ContentType?.ToLower().Contains(JSON_CONTENT_TYPE) ?? false;
        }

        private bool IsJsonResponse(HttpResponse response)
        {
            return response.ContentType?.ToLower().Contains(JSON_CONTENT_TYPE) ?? false;
        }

        private bool IsFileStreamResponse(HttpResponse response)
        {
            var contentType = response.ContentType?.ToLower();
            return contentType?.StartsWith("application/octet-stream") == true
                || contentType?.StartsWith("application/pdf") == true
                || contentType?.StartsWith("image/") == true;
        }
    }

}
