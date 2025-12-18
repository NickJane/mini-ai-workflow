

using Nop.WebApiFramework.Exceptions;

namespace SuperFlowApi.Domain.SuperFlow
{
    /// <summary>
    /// 执行结果
    /// </summary>
    public interface INodeExecuteResult
    {
        /// <summary>
        /// 节点id
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 流式输出委托 - 仅用于 LLMNode 等需要流式输出的节点
        /// 当此属性不为空时，表示该节点支持流式输出
        /// </summary>
        public Func<IAsyncEnumerable<string>>? StreamingExecutor { get; set; }
    }

    public record NodeExecuteResult : INodeExecuteResult
    {
        public string NodeId { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }

        public object? Result { get; set; }
        public string ErrorMsg { get; set; } = string.Empty;
        public int ErrorCode { get; set; }

        /// <summary>
        /// 流式输出委托
        /// </summary>
        public Func<IAsyncEnumerable<string>>? StreamingExecutor { get; set; }

        /// <summary>
        /// 创建成功的执行结果
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="result">执行结果数据</param>
        /// <param name="streamingExecutor">流式输出委托（可选，仅用于 LLMNode 等需要流式输出的节点）</param>
        public static NodeExecuteResult Success(string nodeId, object? result, Func<IAsyncEnumerable<string>>? streamingExecutor = null)
        {
            return new NodeExecuteResult()
            {
                NodeId = nodeId,
                IsSuccess = true,
                Result = result,
                StreamingExecutor = streamingExecutor
            };
        }
        /// <summary>
        /// 创建成功的执行结果
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="result">执行结果数据</param>
        public static NodeExecuteResult Success(string nodeId, object? result = null)
        {
            return new NodeExecuteResult()
            {
                NodeId = nodeId,
                IsSuccess = true,
                Result = result,
                StreamingExecutor = null
            };
        }

        /// <summary>
        /// 创建成功的执行结果（带错误码和消息）
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="result">执行结果数据</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="errorMsg">错误消息</param>
        /// <param name="streamingExecutor">流式输出委托（可选）</param>
        public static NodeExecuteResult Success(string nodeId, object? result, int errorCode = 0, string errorMsg = "", Func<IAsyncEnumerable<string>>? streamingExecutor = null)
        {
            return new NodeExecuteResult()
            {
                NodeId = nodeId,
                IsSuccess = true,
                Result = result,
                ErrorCode = errorCode,
                ErrorMsg = errorMsg,
                StreamingExecutor = streamingExecutor
            };
        }

        public static NodeExecuteResult Error(string nodeId, string errorMsg, int errorCode = 61002)
        {
            return new NodeExecuteResult()
            {
                NodeId = nodeId,
                IsSuccess = false,
                ErrorMsg = errorMsg,
                ErrorCode = errorCode
            };
        }

        public static NodeExecuteResult Error(string nodeId, WebApiException? ex = null)
        {
            return new NodeExecuteResult()
            {
                NodeId = nodeId,
                IsSuccess = false,
                ErrorMsg = ex?.ErrorMessage ?? "node execute failed!",
                ErrorCode = ex == null ? 61002 : ex.ErrorCode
            };
        }
    }
}