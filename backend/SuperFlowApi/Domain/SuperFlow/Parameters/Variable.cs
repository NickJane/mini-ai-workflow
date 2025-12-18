using System.Runtime.Serialization;
using SuperFlowApi.Domain.SuperFlow.Parmeters;

namespace SuperFlowApi.Domain.SuperFlow.Parmeters
{
    /// <summary>
    /// 变量基类
    /// </summary>
    [JsonConverter(typeof(JsonInheritanceConverter), "typeName")]
    [KnownType(typeof(LongVariable))]
    [KnownType(typeof(DecimalVariable))]
    [KnownType(typeof(StringVariable))]
    [KnownType(typeof(BooleanVariable))]
    [KnownType(typeof(DateTimeVariable))]
    [KnownType(typeof(ObjectVariable))]
    [KnownType(typeof(ArrayVariable))]
    public abstract class Variable
    {
        /// <summary>
        /// id
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// 值
        /// </summary>
        [JsonIgnore]
        protected JToken? Value { get; set; } = null;

        /// <summary>
        /// 默认值
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public virtual object? DefaultValue { get; set; }

        /// <summary>
        /// 是否设置已值（第一次成功 setvalue后 为true)
        /// </summary>
        public bool HasValue { get; set; }


        /// <summary>
        /// 设置变量值
        /// </summary>
        /// <param name="token">值</param>
        public virtual bool SetValue(JToken? token, out string? errorMsg)
        {
            errorMsg = null;
            if (IsValid(token).Item1)
            {
                Value = token;
                HasValue = true;
                errorMsg = IsValid(token).Item2;
                return true;
            }
            return false;
        }

        public virtual JToken? GetValue()
        {
            if (HasValue)
            {
                return Value;
            }
            if (DefaultValue == null)
                return null;
            else { 
                var jtoken = JToken.FromObject(DefaultValue);
                var result = IsValid(jtoken);
                if (result.Item1)
                {
                    return jtoken;
                }
                else
                {
                    throw new ArgumentException($"variable {this.Name} default value {DefaultValue?.ToString()} cannot be converted to {this.GetType().Name}");
                }
            }
        }

        public abstract object? GetTypedValue();

        /// <summary>
        /// 如果传入token 以实现类型type校验传入token是否合法，否则校验当前类value是否合法
        /// </summary>
        /// <param name="token">需要校验的值</param>
        /// <returns></returns>
        public abstract (bool, string) IsValid(JToken? token = null);

        public virtual void Reset()
        {
            this.Value = null;
            this.HasValue = false;
        }
    }
}