using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SuperFlowApi.Domain.SuperFlow
{
    public static class JTokenExtension
    {
        /// <summary>
        /// 如果obj本身就是jtoken, 那么则不再fromObject, 会有性能损耗
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static JToken? FromObject(object obj)
        {
            if (obj == null)
                return null;
            if (obj is JToken jt)
                return jt;

            return JToken.FromObject(obj);
        }
        /// <summary>
        /// 如果obj本身就是jtoken, 那么则不再fromObject, 会有性能损耗
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static JToken? TOJToken(this object obj)
        {
            if (obj == null)
                return null;
            return FromObject(obj);
        }
    }

    public static class JsonExtension
    {
        public static bool IsNull(this JToken token)
        {
            if (token == null)
            {
                return true;
            }

            if (token.Type == JTokenType.Null)
            {
                return true;
            }

            if (token is JValue value)
            {
                return value.Value == null;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ja"></param>
        /// <param name="isValueLableObject">当值为true, 则认为此jarry是[{label,value}]数组对象</param>
        /// <returns></returns>
        public static JArray Sort(this JArray ja, bool isValueLableObject = false)
        {
            if (ja == null) return null;
            try
            {
                List<string> lst = new List<string>();
                foreach (var item in ja.Children())
                {
                    if (isValueLableObject)
                    {
                        var label = item.SelectToken("label")?.ToString();
                        var value = item.SelectToken("value")?.ToString();
                        var v = "{\n  \"label\": \"" + label + "\",\n  \"value\": \"" + value + "\"\n}";
                        lst.Add(v);
                    }
                    else
                        lst.Add(item.ToString());
                }
                lst.Sort();

                JArray newArr = new JArray();
                lst.ForEach(x =>
                {
                    newArr.Add(JToken.Parse(x));
                });

                return newArr;
            }
            catch
            {
                return ja;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ja"></param>
        /// <param name="isValueLableObject">当值为true, 则认为此jarry是[{label,value}]数组对象</param>
        /// <returns></returns>
        public static JArray Distinct(this JArray jArray)
        {
            if (jArray == null) return null;
            JArray distinctArr = new JArray();

            foreach (JToken item in jArray)
            {
                bool exists = false;
                foreach (JToken existingItem in distinctArr)
                {
                    if (JToken.DeepEquals(item, existingItem))
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    distinctArr.Add(item);
                }
            }

            return distinctArr;
        }

        /// <summary>
        /// jtoken 逻辑比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool JTokenLogicEquals(this JToken? left, JToken? right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }
            bool result = false;
            switch (left.Type)
            {
                case JTokenType.Integer:
                    result = left.Value<decimal>() == right.Value<decimal>();
                    break;

                case JTokenType.Float:
                    result = left.Value<decimal>() == right.Value<decimal>();
                    break;

                case JTokenType.String:
                    result = left.ToString() == right.ToString();
                    break;

                default:

                    result = JToken.DeepEquals(left, right);
                    break;
            }

            return result;
        }

        /// <summary>
        /// 1 大写, 2,小写, 3,驼峰
        /// </summary>
        /// <param name="token"></param>
        /// <param name="caseOption"></param>
        /// <returns></returns>
        public static JToken ToCase(this JToken token, int caseOption)
        {
            if (token is JObject obj)
            {
                var newObj = new JObject();
                foreach (var property in obj.Properties())
                {
                    string newPropertyName = property.Name;

                    switch (caseOption)
                    {
                        case 1:
                            newPropertyName = property.Name.ToUpper();
                            break;
                        case 2:
                            newPropertyName = property.Name.ToLower();
                            break;
                        case 3:
                            newPropertyName = ToCamelCase(property.Name);
                            break;
                    }

                    newObj[newPropertyName] = property.Value.ToCase(caseOption);
                }
                return newObj;
            }
            else if (token is JArray array)
            {
                var newArray = new JArray();
                foreach (var item in array)
                {
                    newArray.Add(item.ToCase(caseOption));
                }
                return newArray;
            }
            else
            {
                return token;
            }
        }

        private static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            {
                return name;
            }

            // 转换第一个字符为小写
            string camelCaseName = char.ToLower(name[0]) + name.Substring(1);
            return camelCaseName;
        }

        /// <summary>
        /// 判断 JToken 是否可以转换为布尔值，并支持多种表示形式。
        /// </summary>
        /// <param name="token">待判断的 JToken。</param>
        /// <param name="result">输出的布尔值（如果转换成功）。</param>
        /// <returns>如果可以转换为布尔值，则返回 true；否则返回 false。</returns>
        public static bool CanConvertToBool(this JToken token, out bool result)
        {
            result = false;

            if (token == null || token.Type == JTokenType.Null)
                return false;

            switch (token.Type)
            {
                case JTokenType.Boolean:
                    result = token.Value<bool>();
                    return true;

                case JTokenType.Integer:
                    int intValue = token.Value<int>();
                    if (intValue == 1)
                    {
                        result = true;
                        return true;
                    }
                    else if (intValue == 0)
                    {
                        result = false;
                        return true;
                    }
                    break;

                case JTokenType.String:
                    string strValue = token.Value<string>()?.Trim();
                    if (strValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        strValue.Equals("是", StringComparison.OrdinalIgnoreCase) ||
                        strValue.Equals("1", StringComparison.OrdinalIgnoreCase)
                        )
                    {
                        result = true;
                        return true;
                    }
                    else if (strValue.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                             strValue.Equals("否", StringComparison.OrdinalIgnoreCase) ||
                             strValue.Equals("0", StringComparison.OrdinalIgnoreCase)
                             )
                    {
                        result = false;
                        return true;
                    }
                    break;
            }

            return false;
        }


        /// <summary>
        /// 判断 JToken 是否可以转换为 long 类型，并支持小数部分为 0 的情况。
        /// </summary>
        /// <param name="token">待判断的 JToken。</param>
        /// <param name="result">输出的 long 值（如果转换成功）。</param>
        /// <returns>如果可以转换为 long，则返回 true；否则返回 false。</returns>
        public static bool CanConvertToLong(this JToken token, out long result)
        {
            result = 0;

            if (token == null || token.Type == JTokenType.Null)
                return false;

            // If the token is already an integer type
            if (token.Type == JTokenType.Integer)
            {
                result = token.Value<long>();
                return true;
            }

            // If the token is a floating-point number, check if it's a whole number
            if (token.Type == JTokenType.Float)
            {
                double doubleValue = token.Value<double>();
                if (doubleValue % 1 == 0 && doubleValue >= long.MinValue && doubleValue <= long.MaxValue)
                {
                    result = (long)doubleValue;
                    return true;
                }
                return false;
            }

            // If the token is a string, try parsing it
            if (token.Type == JTokenType.String)
            {
                string strValue = token.Value<string>();

                // Try parsing directly as long
                if (long.TryParse(strValue, out result))
                    return true;

                // Try parsing as double and check if it's a whole number
                if (double.TryParse(strValue, out double doubleValue) && doubleValue % 1 == 0)
                {
                    result = (long)doubleValue;
                    return true;
                }
                return false;
            }

            // For other types, return false
            return false;
        }
    }

}
