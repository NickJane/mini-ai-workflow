
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.SuperFlow.Parmeters
{
    /// <summary>
    /// 数值变量
    /// </summary>
    public class LongVariable : Variable
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override (bool, string) IsValid(JToken? token = null)
        {
            var flag = token == null || token.Type == JTokenType.Integer || token.CanConvertToLong(out long _);
            
            if (!flag)
            {
                return (flag, "variable " + this.Name + " value " + token?.ToString() + " cannot be converted to long type");
            }
            return (flag, "");
        }

        /// <summary>
        /// 设置变量值
        /// </summary>
        /// <param name="token">值</param>
        public override bool SetValue(JToken? token, out string? errorMsg)
        {
            errorMsg = null;
            var result = IsValid(token);
            if (result.Item1)
            {
                if (token != null && token.CanConvertToLong(out var n))
                    this.Value = JToken.FromObject(n);
                else
                    this.Value = token;

                HasValue = true;
                return true;
            }
            errorMsg = result.Item2;
            return false;
        }

        /// <summary>
        /// 获取类型化的值
        /// </summary>
        /// <returns></returns>
        public override object? GetTypedValue()
        {
            var v = base.GetValue();
            if (v == null) return null;

            return long.Parse(v.ToString());
        }
    }
}