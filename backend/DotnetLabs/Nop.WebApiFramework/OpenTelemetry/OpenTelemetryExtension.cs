



namespace Nop.WebApiFramework.OpenTelemetry
{
    public static class OpenTelemetryExtension
    {
        public static string? GetRequestId(this HttpContext context)
        {
            if (Activity.Current != null)
            {
                return Activity.Current.TraceId.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        

        public static async Task SetOpenTelemetryTag(this HttpContext context, Activity activity)
        {
            var request = context.Request;
            var response = context.Response;

            var requestHeaders = request.Headers.ToDictionary(c => c.Key.ToString(), c => c.Value.ToString());
            var responseHeaders = response.Headers.ToDictionary(c => c.Key.ToString(), c => c.Value.ToString());

            activity.SetTag("url", HttpUtility.UrlDecode(request.GetDisplayUrl()).ToLowerInvariant());
            activity.SetTag("path", HttpUtility.UrlDecode(request.Path.ToString()).ToLowerInvariant());
            activity.SetTag("contentType", request.ContentType);
            activity.SetTag("queryString", request.QueryString.HasValue ? HttpUtility.UrlDecode(request.QueryString.ToString()) : null);
            activity.SetTag("clientIp", context.GetClientIp());
            if (!context.Request.HasFormContentType) //表单主要为上传类型这里不获取请求内容
            {
                activity.SetTag("requestBody", await context.GetRequestBodyAsync());
            }
            activity.SetTag("requestHeader", requestHeaders != null && requestHeaders.Count > 0 ? HttpUtility.HtmlDecode(JsonConvert.SerializeObject(requestHeaders)) : null);
            activity.SetTag("responseHeader", responseHeaders != null && responseHeaders.Count > 0 ? HttpUtility.HtmlDecode(JsonConvert.SerializeObject(responseHeaders)) : null);
        }


    }
}
