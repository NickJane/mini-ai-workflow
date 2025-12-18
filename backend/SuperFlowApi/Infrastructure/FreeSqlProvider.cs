
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace SuperFlowApi.Infrastructure
{
    public class FreeSqlProvider
    {
        // 缓存已构建的 IFreeSql 实例，避免重复创建
        private readonly ConcurrentDictionary<string, IFreeSql> _fsqlDict = new();
        private readonly ILogger<FreeSqlProvider> _logger;
        private readonly AppSettingsModel _appSettingsModel;

        public FreeSqlProvider(ILogger<FreeSqlProvider> logger, IOptions<AppSettingsModel> options)
        {
            _logger = logger;
            _appSettingsModel = options.Value;
        }

        public IFreeSql GetFreeSql()
        {
            var dbName = "mysqldb";

            var instance = _fsqlDict.GetOrAdd(dbName, _ =>
            {
                
                return GetFreeSqlInner();
            });

            return instance;
        }

        private IFreeSql GetFreeSqlInner()
        {
            // 解析连接字符串
            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                ConnectionString = _appSettingsModel.FreeSqlDatabase.ConnectionString,
            };

            // 生成新的连接字符串
            string newConnectionString = connectionStringBuilder.ConnectionString;

            var builder = new FreeSql.FreeSqlBuilder();
            builder = builder.UseConnectionString(FreeSql.DataType.MySql, newConnectionString);


            builder = builder.UseMonitorCommand(command =>
            {
                _logger.LogDebug("【SQL监控】执行SQL:{SQL}", command.CommandText);
            });

            builder = builder.UseGenerateCommandParameterWithLambda(true);


            builder.UseAutoSyncStructure(true);

            var fsql = builder.Build();

            fsql.UseJsonMap();

            var sqlExecuteElapsedMillisecondsThreshold = 3000;
            //记录耗时SQL
            fsql.Aop.CurdAfter += (s, e) =>
            {
                if (e.Exception != null)
                {
                    _logger.LogError(e.Exception, "【SQL错误】:{message},执行SQL语句:{SQL}", e.Exception.Message, e.Sql);
                }

                if (e.ElapsedMilliseconds > sqlExecuteElapsedMillisecondsThreshold)
                {
                    _logger.LogWarning("【SQL耗时】:{UpElapsedMilliseconds}毫秒,执行SQL:{SQL}", e.ElapsedMilliseconds, e.Sql);
                }
            };

            return fsql;
        }
    }
}
