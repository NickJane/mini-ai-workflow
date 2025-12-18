using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using Nop.WebApiFramework.OpenTelemetry;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Nop.WebApiFramework.ServiceExtentions
{

    public static class OpenTelemetryZipkinExtension
    {
        public static void AddCustomOpenTelemetryZipkinExporter(this WebApplicationBuilder builder, string serviceName)
        {
            builder.Services.Configure<OpenTelemetryOptions>(builder.Configuration.GetSection("OpenTelemetry"));
            var options = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<OpenTelemetryOptions>>();

            if (options.Value != null && options.Value.Enable)
            {
                builder.Logging.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                            .AddConsoleExporter();
                });

                builder.Services.AddOpenTelemetry()
                    .ConfigureResource(resource => resource.AddService(serviceName))
                    .WithTracing(tracing =>
                {
                    var tracerBuilder = tracing
                        .AddHttpClientInstrumentation() // 监控所有 HttpClient 请求; 自动在请求头添加 traceparent 实现跨服务追踪
                        .AddAspNetCoreInstrumentation(c =>// 监控所有抵达asp.net core的请求, 提取请求头中的 TraceId，创建或继承追踪上下文 Activity
                        {
                            c.Filter = context => context.Request.Path.Value == "/hc" ? false : true;
                        })
                        .AddZipkinExporter(c => // 配置Zipkin导出器 发送追踪数据到 Zipkin 服务器
                        {
                            c.Endpoint = new Uri(options.Value.ZipkinEndpoint);
                        });



                    if (options.Value.MySql)
                    {
                        tracerBuilder.AddSource("MySqlConnector");
                    }

                })
                ;
            }
        }

        public static void UseOpenTelemetryCustomTagMiddleware(this WebApplication app)
        {
            var options = app.Services.GetRequiredService<IOptions<OpenTelemetryOptions>>();

            if (options.Value != null && options.Value.Enable)
            {
                app.UseMiddleware<OpenTelemetryCustomTagMiddleware>();
            }
        }
    }
}