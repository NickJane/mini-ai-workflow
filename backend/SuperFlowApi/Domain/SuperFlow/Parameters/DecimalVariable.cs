
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.SuperFlow.Parmeters
{
    /// <summary>
    /// 浮点数变量
    /// </summary>
    public class DecimalVariable : Variable
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override (bool, string) IsValid(JToken? token = null)
        {
            if (token == null) return (true, "");

            if (token.Type == JTokenType.Float || decimal.TryParse(token?.ToString(), out decimal _))
            {
                return (true, "");
            }
            return (false, "variable " + this.Name + " value " + token?.ToString() + " cannot be converted to decimal type");
        }

        /// <summary>
        /// 获取类型化的值
        /// </summary>
        /// <returns></returns>
        public override object? GetTypedValue()
        {
            var v = base.GetValue();
            if (v == null) return null;

            return decimal.Parse(v.ToString());
        }
    }
}