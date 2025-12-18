

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.SuperFlow.Parmeters
{
    /// <summary>
    /// 日期变量
    /// </summary>
    public class DateTimeVariable : Variable
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override (bool, string) IsValid(JToken? token = null)
        {
            if (token == null) return (true, string.Empty);

            if (token.Type != JTokenType.Date && !DateTime.TryParse(token?.ToString(), out _)) 
                return (false, $"variable {Name} value {token?.ToString()} cannot be converted to date type");

            return (true, string.Empty);
        }
        public override bool SetValue(JToken? token, out string error)
        {
            (bool, string) isValid = IsValid(token);
            if (isValid.Item1)
            {
                Value = token;
                HasValue = true;
                error = string.Empty;
                return true;
            }
            error = isValid.Item2;
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

            return DateTime.Parse(v.ToString());
        }
    }
}