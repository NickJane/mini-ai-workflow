

using SuperFlowApi.Domain.SuperFlow.Nodes;
using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Entities;

namespace SuperFlowApi.Domain.SuperFlowAIRun
{
    /// <summary>
    /// 流程运行时服务
    /// </summary>
    public class FlowRuntimeAIService : FlowRuntimeService
    {
        private List<string> _logsOfCurrentNode;
        private INodeExecuteResult _currentNodeExecuteResult;

        /// <summary>
        /// 节点执行结果缓存 - 用于节点间引用输出数据
        /// Key: NodeId, Value: 节点执行结果
        /// </summary>
        private readonly Dictionary<string, INodeExecuteResult> _nodeExecuteResults;
        private StringBuilder _sessionFullResponseText;

        /// <summary>
        /// 流式输出回调 - 用于将流式数据推送给客户端
        /// 回调参数：(eventType, nodeId, data)
        /// eventType: "message" | "node_started" | "node_finished"
        /// </summary>
        public Func<string, string, object, Task>? StreamingOutputCallback { get; set; }

        public FlowRuntimeAIService(IServiceProvider serviceProvider, IFreeSql freeSql)
        : base(serviceProvider, freeSql)
        {
            _nodeExecuteResults = new();
            _sessionFullResponseText = new();
        }


        public override Task<INodeExecuteResult?> GetNodeExecuteResult(FlowRuntimeContext context, string nodeId)
        {
            _nodeExecuteResults.TryGetValue(nodeId, out var result);
            return Task.FromResult(result);
        }

        public override async Task RunAsync(FlowRuntimeContext context)
        {
            await RunInnerAsync(context as FlowRuntimeAIContext);
        }

        private async Task RunInnerAsync(FlowRuntimeAIContext context)
        {
            var startNodes = context.FlowConfigInfoForRun.Nodes.Where(x => x is StartNode).ToList();
            if (startNodes.Count != 1)
                throw new WebApiException("start node count must be 1");

            var startNode = startNodes[0];
            

            // 记录开始节点日志
            CurrentNode = startNode;
            var excuteResult = await startNode.ExecuteAsync(context, this);


            // 执行流程
            var isSuccess = await RunNextNodeAsync(context, startNode);

            // 会话持久化
            await SessionManager(context);
        }


        private async Task<bool> RunNextNodeAsync(FlowRuntimeContext context, NodeBase nodeExcuted)
        {
            // 记录当前节点日志
            _logsOfCurrentNode = new List<string>();
            DateTime startTime = DateTime.Now;
            var isSuccess = false;

            NodeLine? nextNodeLine = null;

            try
            {
                nextNodeLine = await nodeExcuted.GetExecuteLine(context, this);

                if (nextNodeLine != null)
                {
                    CurrentNode = context.FlowConfigInfoForRun.Nodes.FirstOrDefault(x => x.Id == nextNodeLine.ToNodeId);

                    if (CurrentNode != null)
                    {
                        _currentNodeExecuteResult = await CurrentNode.ExecuteAsync(context, this);

                        _nodeExecuteResults[CurrentNode.Id] = _currentNodeExecuteResult;

                        if (CurrentNode is ReplyNode && _currentNodeExecuteResult.StreamingExecutor != null)
                        {
                            var returnText = await ExecuteStreamingOutput(CurrentNode.Id, _currentNodeExecuteResult.StreamingExecutor);
                            _sessionFullResponseText.Append(returnText);
                        }

                        isSuccess = _currentNodeExecuteResult.IsSuccess;

                        if (isSuccess == false)
                            throw new WebApiException($"node {CurrentNode.DisplayName} execute failed: {_currentNodeExecuteResult.ErrorMsg}");
                        else
                            await CurrentNode.CallbackAfterExecuteAsync(context, this);

                    }
                    else
                    {
                        throw new WebApiException($"can not find node {nextNodeLine.ToNodeId}");
                    }
                }
            }
            catch (WebApiException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebApiException($"node {CurrentNode?.DisplayName} execute failed: {ex.Message}");
            }
            finally
            {
                if (isSuccess)
                {
                    await RunNextNodeAsync(context, CurrentNode);
                }
            }

            return isSuccess;

        }


        private async Task<string> ExecuteStreamingOutput(string nodeId, Func<IAsyncEnumerable<string>> streamingExecutor)
        {
            StringBuilder fullResponse = new();

            await foreach (var chunk in streamingExecutor())
            {
                if (chunk is string strChunk)
                {
                    fullResponse.Append(strChunk);
                    await StreamingOutputCallback("message", nodeId, strChunk);
                }
            }
            return fullResponse.ToString();
        }



        private async Task SessionManager(FlowRuntimeAIContext context)
        {

            using (var uow = _freeSql.CreateUnitOfWork())
            {
                var conversation = await uow.Orm.Select<FlowChatConversationEntity>().Where(x => x.ConversationId == context.Request.ConversationId).FirstAsync();
                if (conversation == null)
                {
                    conversation = new FlowChatConversationEntity()
                    {
                        ConversationId = context.Request.ConversationId,
                        User = context.User,
                        FlowId = context.FlowId,
                        Title = context.Request.Query.Length > 10 ? context.Request.Query.Substring(0, 10) : context.Request.Query,
                        IsTop = false,
                        // 当下完成第一轮会话
                        MessageCount = 1,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await uow.Orm.Insert(conversation).ExecuteAffrowsAsync();
                }
                else
                {
                    conversation.MessageCount++;
                    conversation.UpdatedAt = DateTime.Now;

                    await uow.Orm.Update<FlowChatConversationEntity>().SetSource(conversation).ExecuteAffrowsAsync();
                }

                var flowChatMessage = new FlowChatMessageEntity()
                {
                    Id = SnowflakeId.NextId(),
                    ConversationId = context.Request.ConversationId,
                    User = context.User,
                    FlowInstanceId = context.FlowInstanceId,
                    FlowId = context.FlowId,

                    Question = context.Request.Query,
                    Answer = _sessionFullResponseText.ToString(),

                };
                await uow.Orm.Insert(flowChatMessage).ExecuteAffrowsAsync();

                uow.Commit();
            }
        }
    }
}