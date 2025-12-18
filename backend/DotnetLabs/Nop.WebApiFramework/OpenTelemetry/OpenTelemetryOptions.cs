using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.WebApiFramework.OpenTelemetry
{
    public class OpenTelemetryOptions
    {
        public OpenTelemetryOptions()
        {
            Enable = true;
            MaxResponseSizeInBytes = 1024;
            Mongodb = false;
            MySql = false;
            EnableResponse = true;
            ZipkinEndpoint = "http://localhost:9411/api/v2/spans";
        }

        /// <summary>
        /// 是否启用可观察性,默认开启
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 最大响应字节,默认1024
        /// </summary>
        public int MaxResponseSizeInBytes { get; set; }
        /// <summary>
        /// 是否启用Mongodb的执行语句监控,默认false
        /// </summary>
        public bool Mongodb { get; set; }
        /// <summary>
        /// 是否启用MySql的执行语句监控,默认false
        /// </summary>
        public bool MySql { get; set; }
        /// <summary>
        /// 链路跟踪Zipkin地址,默认http://localhost:9411/api/v2/spans
        /// </summary>
        public string ZipkinEndpoint { get; set; }
        /// <summary>
        /// 是否启用响应内容数据收集
        /// </summary>
        public bool? EnableResponse { get; set; }
    }
}
