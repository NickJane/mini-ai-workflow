using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;
using Nop.WebApiFramework.Pipeline;
using Nop.WebApiFramework.Serilogs;
using Nop.WebApiFramework.ServiceExtentions;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using Nop.Infrastructure.Json;
using SuperFlowApi;
using Microsoft.AspNetCore.Mvc;
using Nop.WebApiFramework.Exceptions;
using System.Text;

// 服务名称
string serviceName = "superflowapi";
var builder = WebApplication.CreateBuilder(args);

// 注册对 GBK 编码支持, 有些股票接口返回是GBK编码的
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// 基础配置：日志、配置文件、依赖注入容器, AddHttpContextAccessor, httpclient, 内存缓存等
builder.NetCoreBasic(serviceName, typeof(EnumApiGroupNames));

var services = builder.Services;
var configuration = builder.Configuration;


// 注入配置项
services.Configure<AppSettingsModel>(configuration);
// 读取配置
var appSettingsModel = configuration.Get<AppSettingsModel>();

// --------------关注业务和存储配置即可
// 添加自定义服务
services.AddCustomServices(appSettingsModel);
// JWT 认证
services.AddJwtAuth(configuration);
// 数据库配置
services.AddSingleton<FreeSqlProvider>();
// services.AddCustomMySql(appSettingsModel, x => { });
// 控制器配置（添加用户认证 Filter）
services.AddControllers(options =>
{
    // 全局过滤器, 添加用户认证 Filter
    options.Filters.Add<Nop.WebApiFramework.UserAccount.UserAuthenticationFilter>();
})
.AddControllersAsServices()
.ConfigureCustomApiBehaviorOptions()
.AddCustomNewtonsoftSupport();
// --------------关注业务和存储配置即可




// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
// 这个方法在启动中执行, 且执行一次, 并在这里设置http请求的管道组件.组件分顺序.
var app = builder.Build();

// 配置转发头，支持代理和负载均衡器
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
    // 信任的代理服务器数量（根据实际部署情况调整）
    ForwardedForHeaderName = "X-Forwarded-For",
    ForwardedProtoHeaderName = "X-Forwarded-Proto",
    ForwardedHostHeaderName = "X-Forwarded-Host"
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // app.UseExceptionHandler("/Error"); // 不再需要, 已全局处理
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // app.UseCustomSwagger(typeof(EnumApiGroupNames));
}
app.UseCustomSwagger(typeof(EnumApiGroupNames));

// 添加自定义扩展
app.UseWebApiFrameWork(serviceName);

// 让其支持Sockets
app.UseWebSockets();

app.UseRouting();

app.UseAuthentication();// 必须要加这个, 不然Bearer token验证不了
app.UseAuthorization(); // 启动权限判断 [Authorize] 生效


app.MapControllers();

app.Run();
