using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperFlowApi.Domain.UserAccount;


namespace SuperFlowApi
{
    /// <summary>
    /// 程序扩展类，用于配置自定义服务
    /// </summary>
    public static class ProgramExtentions
    {
        /// <summary>
        /// 添加自定义服务
        /// </summary>
        public static void AddCustomServices(this IServiceCollection services, AppSettingsModel appSettingsModel)
        {

            // JWT Token 提供者
            services.AddScoped<LoginManager>();
            services.AddScoped<JwtTokenProvider>();


            services.AddScoped<UserService>();

        }

    }
}
