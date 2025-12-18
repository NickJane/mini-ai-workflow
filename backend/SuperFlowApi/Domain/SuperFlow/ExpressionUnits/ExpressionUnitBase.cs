using SuperFlowApi.Domain.SuperFlow.Nodes;

namespace SuperFlowApi.Domain.SuperFlow.ComputeUnits
{
    /// <summary>
    /// 计算单元基类
    /// </summary>
    [JsonConverter(typeof(JsonInheritanceConverter), "typeName")]
    [KnownType(typeof(JSExpressionUnit))]
    [KnownType(typeof(FullTextExpressionUnit))]
    [KnownType(typeof(FullTextMiniExpressionUnit))]
    public abstract class ExpressionUnitBase
    {
        /// <summary>
        /// id
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 计算单元计算项
        /// </summary>
        public virtual List<ExpressionUnitBase> Children { get; set; } = new List<ExpressionUnitBase>();

        /// <summary>
        /// 执行计算动作
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract Task<object?> ComputeValue(FlowRuntimeContext context, FlowRuntimeService service);

        public virtual async Task<T?> ComputeValueAs<T>(FlowRuntimeContext context, FlowRuntimeService service)
        {
            object? originValue = await ComputeValue(context, service);
            if (originValue != null)
            {
                if (originValue is ValueType)
                {
                    // 注: 类型为值类型, 如果不能
                    return JToken.FromObject(originValue).Value<T>();
                }
                return JToken.FromObject(originValue).ToObject<T>() ?? default;
            }
            else
            {
                return default;
            }
        }
    }
}