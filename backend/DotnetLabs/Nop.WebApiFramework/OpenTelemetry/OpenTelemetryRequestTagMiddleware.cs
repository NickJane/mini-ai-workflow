using Microsoft.Extensions.Options;
using Nop.Infrastructure;

namespace Nop.WebApiFramework.OpenTelemetry;


/// <summary>
/// 自定义链路跟踪标签
/// </summary>
public class OpenTelemetryCustomTagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly OpenTelemetryOptions _options;

    public OpenTelemetryCustomTagMiddleware(RequestDelegate next,
         IOptions<OpenTelemetryOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    private static List<string> NoTraceList = new List<string>()
    {
        "hangfire"
    };
    public async Task InvokeAsync(HttpContext context)
    {
        var flag = Activity.Current != null && Activity.Current.Source.Name == OpenTelemetryConst.OpenTelemetryInstrumentationAspNetCore;
        if (flag && !NoTraceList.Contains(context.Request.Path.Value?.ToLower()))
        {
            var activity = Activity.Current;

            // 在链路跟踪设置自定义属性
            await context.SetOpenTelemetryTag(activity);

            if (_options.EnableResponse.HasValue && _options.EnableResponse.Value)
            {
            }
        }
        
        // 继续执行下一个中间件
        await _next(context);
        
    }
}

