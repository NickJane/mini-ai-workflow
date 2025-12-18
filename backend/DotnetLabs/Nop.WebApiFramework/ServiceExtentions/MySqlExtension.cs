using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nop.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Nop.WebApiFramework.ServiceExtentions
{
    public static class MySqlExtension
    {
        public static void AddCustomMySql(this IServiceCollection services, AppSettingsModel model, Action<FreeSql.FreeSqlBuilder> action)
        {
            Func<IServiceProvider, IFreeSql> fsqlFactory = r =>
            {
                var logger = r.GetRequiredService<ILogger<IFreeSql>>();
                var builder = new FreeSql.FreeSqlBuilder();
                builder = builder.UseConnectionString(FreeSql.DataType.MySql, model.FreeSqlDatabase.ConnectionString);

                if (model.FreeSqlDatabase.UseMonitor)
                {
                    builder = builder.UseMonitorCommand(command =>
                    {
                        logger.LogDebug("【SQL监控】执行SQL:{SQL}", command.CommandText);
                    });
                }

                if (model.FreeSqlDatabase.UseGenerateCommandParameterWithLambda)
                {
                    builder = builder.UseGenerateCommandParameterWithLambda(true);
                }

                builder.UseAutoSyncStructure(true);

                action?.Invoke(builder);

                var fsql = builder.Build();

                fsql.UseJsonMap(JsonConvert.DefaultSettings());


                var sqlExecuteElapsedMillisecondsThreshold = model.FreeSqlDatabase.SqlExecuteElapsedMillisecondsThreshold.GetValueOrDefault(3000);
                //记录耗时SQL
                fsql.Aop.CurdAfter += (s, e) =>
                {
                    if (e.Exception != null)
                    {
                        logger.LogError(e.Exception, "【SQL错误】:{message},执行SQL语句:{SQL}", e.Exception.Message, e.Sql);
                    }

                    if (e.ElapsedMilliseconds > sqlExecuteElapsedMillisecondsThreshold)
                    {
                        logger.LogWarning("【SQL耗时】:{UpElapsedMilliseconds}毫秒,执行SQL:{SQL}", e.ElapsedMilliseconds, e.Sql);
                    }
                };


                return fsql;
            };
            services.AddSingleton<IFreeSql>(fsqlFactory);
        }
    }
}