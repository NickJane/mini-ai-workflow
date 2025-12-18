
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.SuperFlow.Parmeters
{
    /// <summary>
    /// 布尔变量
    /// </summary>
    public class BooleanVariable : Variable
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override (bool, string) IsValid(JToken? token = null)
        {
            var flag = token == null || token.CanConvertToBool(out var _);

            if (!flag)
            {
                return (flag, "variable " + this.Name + " value " + token?.ToString() + " cannot be converted to boolean type");
            }
            return (flag, "");
        }

        /// <summary>
        /// 设置变量值
        /// </summary>
        /// <param name="token">值</param>
        public override bool SetValue(JToken? token, out string? errorMsg)
        {
            var result = IsValid(token);
            errorMsg = result.Item2;
            if (result.Item1)
            {
                if (token != null && token.CanConvertToBool(out var n))
                    this.Value = JToken.FromObject(n);
                else
                    this.Value = token;

                HasValue = true;
                return true;
            }
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

            return bool.Parse(v.ToString());
        }
    }
}