
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.SuperFlow.Parmeters
{
    /// <summary>
    /// 文本变量
    /// </summary>
    public class StringVariable : Variable
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override (bool,string) IsValid(JToken? token = null)
        {
            //if (token == null || token.Type != JTokenType.String) return false;

            return (true, "");
        }

        /// <summary>
        /// 获取类型化的值
        /// </summary>
        /// <returns></returns>
        public override object? GetTypedValue()
        {
            return base.GetValue()?.ToString();
        }
    }
}