






using System.IO.Compression;

namespace Nop.WebApiFramework;
public class Body
{
    public string Content { get; set; } = null!;
}
public static class HttpContextExtension
{
    /// <summary>
    /// 获取请求报文; 如果Feature中存在请求报文则直接返回, 没有则读取请求报文并写入Feature
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static async Task<string> GetRequestBodyAsync(this HttpContext httpContext)
    {
        if ((httpContext.Request.ContentType?.ToLower().Contains("application/json") ?? false) == false)
            return string.Empty;

        var body = httpContext.GetFeatureBody();

        if (body != null) return body.Content;

        var bodyString = string.Empty;

        httpContext.Request.EnableBuffering();

        if (httpContext.Request.ContentLength > 0 && httpContext.Request.Body.CanRead)
        {
            var stream = httpContext.Request.Body;
            using (var ms = new MemoryStream())
            {
                stream.Position = 0;
                await stream.CopyToAsync(ms);
                ms.Position = 0;
                bodyString = Encoding.UTF8.GetString(ms.ToArray());
            }
            httpContext.Request.Body.Position = 0;
        }

        SetFeatureBody(httpContext, bodyString);

        return bodyString;
    }

    /// <summary>
    /// 获取响应内容
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<string> GetJsonResponseBodyAsync(this HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Response.ContentType) || !context.Response.ContentType.Contains("application/json"))
        {
            if (context.Response.Body.CanSeek)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
            }
            return string.Empty;
        }

        if (FilterStaticFiles(context))
        {
            if (context.Response.Body.CanSeek)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
            }

            return string.Empty;
        }

        string result = string.Empty;

        context.Response.Body.Seek(0, SeekOrigin.Begin);

        Stream source = null;

        if (context.Response.Headers.ContainsKey("Content-Encoding"))
        {
            var contentEncoding = context.Response.Headers["Content-Encoding"].ToString();
            switch (contentEncoding)
            {
                case "gzip":
                    source = new GZipStream(context.Response.Body, CompressionMode.Decompress);
                    break;
                case "deflate":
                    source = new DeflateStream(context.Response.Body, CompressionMode.Decompress);
                    break;
            }
        }

        if (source == null)
        {
            source = context.Response.Body;
        }

        var responseReader = new StreamReader(source, System.Text.Encoding.UTF8);

        result = await responseReader.ReadToEndAsync();

        context.Response.Body.Seek(0, SeekOrigin.Begin);

        return HttpUtility.HtmlDecode(result);
    }


    /// <summary>
    /// 获取客户端Ip
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetClientIp(this HttpContext context)
    {
        string UnknowIP = "0.0.0.0";
        var ip = context.Request?.Headers["X-Ip"].FirstOrDefault();//自定义请求头IP必须放置在最前面,由于安全性规则无法自定义写入X-Forwarded-For,故加上自定义X-Ip
        if (string.IsNullOrEmpty(ip))
            ip = context.Request?.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
            ip = context.Request?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4()?.ToString();
        return ip ?? UnknowIP;
    }


    /// <summary>
    /// 从管道Feature中拿到请求报文
    /// </summary>
    /// <param name="context"></param>
    /// <param name="content"></param>
    public static void SetFeatureBody(this HttpContext context, string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            context.Features.Set<Body>(new Body { Content = content });
        }
    }

    /// <summary>
    /// 将请求报文写入Feature
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Body? GetFeatureBody(this HttpContext context)
    {
        return context.Features.Get<Body>();
    }

    /// <summary>
    /// 获取Header=>UserId
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static long GetUserId(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("UserId", out StringValues userId) &&
        long.TryParse(userId.ToString(), out long id))
        {
            return id;
        }
        return 0;
    }

    /// <summary>
    /// 获取公共请求头
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetCommonRequestHeader(this HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException("context");

        var header = new Dictionary<string, string>();

        if (context.Request.Headers.TryGetValue("authorization", out StringValues authorization))
            header.Add("authorization", authorization.ToString());

        if (context.Request.Headers.TryGetValue("userId", out StringValues userId))
            header.Add("userId", userId.ToString());

        if (context.Request.Headers.TryGetValue("sso", out StringValues sso))
            header.Add("sso", sso.ToString());

        return header;
    }




    /// <summary>
    /// 过滤Options请求
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static bool FilterStaticFiles(HttpContext context)
    {
        if (context.Request.Method.ToLowerInvariant() == "options")
            return true;

        return false;
    }
    public static string FullUrl(this HttpRequest request)
    {
        var path = request.Path.HasValue ? request.Path.Value : "/";

        var str = $"{request.Scheme}://{request.Host.Host}:{request.Host.Port ?? 80}{path}{request.QueryString}";

        return str;
    }

    public static string ToCurl(this HttpRequest request, string body = "")
    {
        string curl = "curl ";
        if (request.Method != "GET")
            curl += $"-X {request.Method} ";

        curl += request.FullUrl();

        if (!string.IsNullOrEmpty(body))
        {
            curl += $" -d '{body}' ";
        }

        foreach (var header in request.Headers)
        {
            curl += $" -H '{header.Key}:{header.Value}'";
        }


        return curl;
    }

}
