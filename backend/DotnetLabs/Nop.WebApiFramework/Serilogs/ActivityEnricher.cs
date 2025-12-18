

using System.Collections.Generic;
using System.Diagnostics;

namespace Nop.WebApiFramework.Serilogs
{
    /// <summary>
    /// 添加自定义日志到serilog中
    /// </summary>
    public class ActivityEnricher : ILogEventEnricher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _appName;
        public ActivityEnricher(IServiceProvider serviceProvider, string appName)
        {
            _serviceProvider = serviceProvider;
            _appName = appName;
        }
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;
            if (activity != null && activity.Id != null)
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty("RequestId", new ScalarValue(activity.Id?.ToString())));
                logEvent.AddOrUpdateProperty(new LogEventProperty("SpanId", new ScalarValue(activity.SpanId.ToString())));
                logEvent.AddOrUpdateProperty(new LogEventProperty("TraceId", new ScalarValue(activity.TraceId.ToString())));
                logEvent.AddOrUpdateProperty(new LogEventProperty("RootId", new ScalarValue(activity.RootId?.ToString())));
                logEvent.AddOrUpdateProperty(new LogEventProperty("ParentId", new ScalarValue(activity.ParentId?.ToString())));
            }

            logEvent.AddOrUpdateProperty(new LogEventProperty("AppName", new ScalarValue(_appName)));

            var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();

            if (httpContextAccessor != null && httpContextAccessor.HttpContext != null)
            {
                var headers = new Dictionary<string, string>();

                foreach (var header in headers)
                {
                    logEvent.AddOrUpdateProperty(new LogEventProperty(header.Key, new ScalarValue(header.Value)));
                }
            }
        }
    }
}
