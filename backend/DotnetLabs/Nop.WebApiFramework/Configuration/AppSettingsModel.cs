using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.WebApiFramework.configuration;

namespace Nop.WebApiFramework
{
    public partial class AppSettingsModel
    {
        public string? ServiceName { get; set; }
        public Logging Logging { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }

        public FreeSqlDatabaseSettings FreeSqlDatabase { get; set; }

        public JwtSettingModel JwtSetting { get; set; }

    }

    public partial class ConnectionStrings
    {
        public string RedisConnectString { get; set; }
        public string SqlServerConn { get; set; }
        public string MySqlConn { get; set; }
        public string PostgresConn { get; set; }

    }

    public class Logging
    {
        public bool IncludeScopes { get; set; }
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string? Default { get; set; }
        public string System { get; set; }
        public string Microsoft { get; set; }
    }



    public class FreeSqlDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string MainDatabaseName { get; set; } = null!;

        /// <summary>
        /// SQL执行耗时监控阈值
        /// </summary>
        public int? SqlExecuteElapsedMillisecondsThreshold { get; set; }

        /// <summary>
        /// 是否监控SQL执行命令
        /// </summary>
        public bool UseMonitor { get; set; } = false;


        /// <summary>
        /// LINQ是否参数化
        /// </summary>
        public bool UseGenerateCommandParameterWithLambda { get; set; } = true;
        /// <summary>
        /// 是否使用ADO连接池
        /// </summary>
        public bool UseAdoConnectionPool { get; set; } = false;

    }
}
