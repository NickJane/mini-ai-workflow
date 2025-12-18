
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using Nop.Infrastructure;
using Nop.WebApiFramework.OpenTelemetry;
using Nop.WebApiFramework.Pipeline;
using Nop.WebApiFramework.Serilogs;
using Nop.WebApiFramework.ServiceExtentions;
using Serilog;

namespace Nop.WebApiFramework;

public static class ProgramExtensions
{
    public static void NetCoreBasic(this WebApplicationBuilder builder, string serviceName, Type swaggerApiGroup = null)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;


        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxConcurrentConnections = 10000;        
            options.Limits.MaxConcurrentUpgradedConnections = 10000; 
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(20); 
        });

        // 注入httpcontext, 内存缓存, httpclient
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddHttpClient();

        // 跨域设置
        builder.AddCustomCors();

        // 添加链路跟踪和seq/zipkin
        builder.AddCustomOpenTelemetryZipkinExporter(serviceName);
        builder.Host.UseSerilog((ctx, config) =>
        {
            var logger = config.ReadFrom.Configuration(ctx.Configuration);
            logger.Enrich.With(new ActivityEnricher(builder.Services.BuildServiceProvider(), serviceName));
        });
        // 添加swagger 

        services.AddCustomSwagger(swaggerApiGroup);
    }


    public static IMvcBuilder ConfigureCustomApiBehaviorOptions(this IMvcBuilder builder, Action<ApiBehaviorOptions>? setupAction = null)
    {
        Action<ApiBehaviorOptions> customOptions = (options) =>
        {
            options.InvalidModelStateResponseFactory = (context) =>
            {
                var error = context.ModelState.GetValidationSummary();

                return new BadRequestObjectResult(new JsonResponse(500, error ?? "error", context.HttpContext.GetRequestId()));
            };
        };

        if (setupAction != null)
        {
            customOptions += setupAction;
        }
        return builder.ConfigureApiBehaviorOptions(customOptions);
    }


    public static void AddCustomCors(this WebApplicationBuilder builder, string policyName = "CorsPolicy")
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(policyName, builder =>
                builder
                .SetIsOriginAllowed(o => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetPreflightMaxAge(TimeSpan.FromHours(1))  
            );
        });
    }

    public static void UseCustomCors(this WebApplication app, string policyName = "CorsPolicy")
    {
        app.UseCors(policyName);
    }


    public static void UseWebApiFrameWork(this WebApplication app, string appName)
    {
        app.UseMiddleware<HttpRequestBodyMiddleware>(); 

        app.UseMiddleware<GlobalExceptionMiddleware>(appName);

        app.UseOpenTelemetryCustomTagMiddleware();

        app.UseCustomCors();
    }




    private static string? GetValidationSummary(this ModelStateDictionary modelState, string separator = "\r\n")
    {
        if (modelState.IsValid) return null;

        var error = new StringBuilder();
        error.Append("ModelState is not valid: ");

        foreach (var item in modelState)
        {
            var state = item.Value;
            var message = state.Errors.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ErrorMessage))?.ErrorMessage;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = state.Errors.FirstOrDefault(o => o.Exception != null)?.Exception?.Message;
            }
            if (string.IsNullOrWhiteSpace(message)) continue;

            if (error.Length > 0)
            {
                error.Append(separator);
            }

            error.Append(message);
        }

        return error.ToString();
    }


}