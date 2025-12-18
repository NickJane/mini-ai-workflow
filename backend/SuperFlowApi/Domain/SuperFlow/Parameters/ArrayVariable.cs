


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nop.WebApiFramework.Exceptions;

namespace SuperFlowApi.Domain.SuperFlow.Parmeters
{
    /// <summary>
    /// 变量类型
    /// </summary>
    public enum VariableItemType
    {
        LongVariable,
        DecimalVariable,
        /// <summary>
        /// 文本入参
        /// </summary>
        StringVariable,

        /// <summary>
        /// 对象入参
        /// </summary>
        ObjectVariable,

        /// <summary>
        /// 日期时间入参
        /// </summary>
        DateTimeVariable,

        /// <summary>
        /// 布尔入参
        /// </summary>
        BooleanVariable,
    }

    /// <summary>
    /// 数组变量
    /// </summary>
    public class ArrayVariable : Variable
    {
        public VariableItemType? ItemType { get; set; }

        /// <summary>
        /// 数组元素
        /// </summary>
        public List<Variable> Children { get; set; } = new List<Variable>();

        /// <summary>
        /// 是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override (bool, string) IsValid(JToken? token = null)
        {
            var flag = token != null && token.Type == JTokenType.Array;
            return (flag, flag ? "" : "variable " + this.Name + " value " + token?.ToString() + " cannot be converted to array type");
        }

        /// <summary>
        /// 获取类型化的值
        /// </summary>
        /// <returns></returns>
        public override object? GetTypedValue()
        {
            return base.GetValue();
        }

        /// <summary>
        /// 设置数组值
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool SetValue(JToken? token, out string? errorMsg)
        {
            var result = IsValid(token);
            errorMsg = result.Item2;
            if (result.Item1)
            {
                if (ItemType == VariableItemType.ObjectVariable && Children.Any())
                {
                    Value = token == null ? null : JArray.FromObject(JArray.FromObject(token).Select(x => BuildItemObject(x)));
                }
                else
                {
                    Value = token;
                }
                HasValue = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 构建对象数组定义对象
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public JObject? BuildItemObject(JToken? source)
        {
            JObject jo = new JObject();
            foreach (var property in Children)
            {
                if (property.SetValue(source?.SelectToken(property.Name), out var errorMsg))
                {
                    jo.Add(new JProperty(property.Name, property.GetTypedValue()));
                }
                else
                {
                    throw new WebApiException($"variable {this.Name} property {property.Name} set failed: " + errorMsg);
                }
            }
            return jo;
        }
    }
}