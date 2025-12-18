using SuperFlowApi.Domain.SuperFlow.Nodes;

namespace SuperFlowApi.Domain.SuperFlow.ComputeUnits
{
    public class FullTextExpressionUnit : ExpressionUnitBase
    {
        public string Text { get; set; } = string.Empty;

        public override async Task<object?> ComputeValue(FlowRuntimeContext context, FlowRuntimeService service)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return string.Empty;
            }

            try
            {
                return await ExpressionHelper.ReplacePlaceholdersAsync(context, service, Text);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"full text expression unit compute value error: {ex.Message}", ex);
            }
        }
    }

    public class FullTextMiniExpressionUnit : FullTextExpressionUnit{
        public override async Task<object?> ComputeValue(FlowRuntimeContext context, FlowRuntimeService service)
        {
            return await base.ComputeValue(context, service);
        }
    }
}