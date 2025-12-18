using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using Nop.WebApiFramework.UserAccount.Jwt.Model;

namespace Nop.WebApiFramework.UserAccount
{

    public class AppJwtBearerEvents : JwtBearerEvents
    {
        public override Task MessageReceived(MessageReceivedContext context)
        {
            // 从 Http Request Header 中获取 Authorization
            string authorization = context.Request.Headers[HeaderNames.Authorization];
            if (string.IsNullOrEmpty(authorization))
            {
                context.NoResult();
                return Task.CompletedTask;
            }

            // 必须为 Bearer 认证方案
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // 赋值token
                context.Token = authorization["Bearer ".Length..].Trim();
            }

            if (string.IsNullOrEmpty(context.Token))
            {
                context.NoResult();
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public override Task TokenValidated(TokenValidatedContext context)
        {
            Console.WriteLine("AppJwtBearerEvents:token验证通过后回调。");
            

            return Task.CompletedTask;
        }

        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            Console.WriteLine($"AppJwtBearerEvents: 由于认证过程中抛出异常，导致身份认证失败后回调。Exception: {context.Exception}");

            return Task.CompletedTask;
        }

        public override Task Challenge(JwtBearerChallengeContext context)
        {
            Console.WriteLine($"AppJwtBearerEvents:质询时回调。Authenticate Failure: {context.AuthenticateFailure}");
            Console.WriteLine($"AppJwtBearerEvents:质询时回调。Error: {context.Error}");
            Console.WriteLine($"AppJwtBearerEvents:质询时回调。Error Description: {context.ErrorDescription}");
            Console.WriteLine($"AppJwtBearerEvents:质询时回调。Error Uri: {context.ErrorUri}");

            return Task.CompletedTask;
        }

        public override Task Forbidden(ForbiddenContext context)
        {
            Console.WriteLine("AppJwtBearerEvents:当出现403（Forbidden，禁止）时回调。");
            return Task.CompletedTask;
        }
    }

}
