namespace SuperFlowApi.Domain.SuperFlow.Parmeters
{
    /// <summary>
    /// 对象变量
    /// </summary>
    public class ObjectVariable : Variable
    {
        /// <summary>
        /// 属性
        /// </summary>
        public List<Variable> Children { get; set; } = new List<Variable>();

        /// <summary>
        /// 默认值
        /// </summary>
        public override object? DefaultValue { get; set; } = null;

        public override (bool, string) IsValid(JToken? token)
        {
            if (base.Required && token == null)
            {
                return (false, $"{this.Name} cannot be null");
            }
            if (token != null && token.Type != JTokenType.Object)
            {
                return (false, $"{this.Name} type error");
            }
            

            foreach (var property in Children)
            {
                var propertyToken = token?.SelectToken(property.Name);

                if (!string.IsNullOrEmpty(propertyToken?.ToString()))
                {
                    // 如果有值, 则进入值合法性验证
                    if (propertyToken != null)
                    {
                        var (propertyIsValid, propertyErrorMsg) = property.IsValid(propertyToken);
                        if (!propertyIsValid)
                        {
                            return (false, $"{this.Name} object property {propertyErrorMsg}");
                        }
                    }
                }
            }
            return (true, string.Empty);
        }


        /// <summary>
        /// 获取类型化的值
        /// </summary>
        /// <returns></returns>
        public override object? GetTypedValue()
        {
            return this.GetValue();
        }


        /// <summary>
        /// 获取值
        /// </summary>
        /// <returns></returns>
        public override JToken? GetValue()
        {
            var val = base.GetValue();
            return val;
        }

    }
}