using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Dtos;
using SuperFlowApi.Infrastructure;
using Nop.WebApiFramework.Exceptions;
using Nop.WebApiFramework.UserAccount.Jwt.Model;
using SuperFlowApi.Domain.SuperFlowAIRun;
using SuperFlowApi.Domain.SuperFlow.Entities;

namespace SuperFlowApi.Controllers.SuperFlow
{
    public partial class FlowController
    {
        /// <summary>
        /// 流式对话-发送流式聊天请求
        /// </summary>
        /// <param name="flowId">流程ID</param>
        /// <param name="request">聊天请求</param>
        /// <returns>流式聊天响应</returns>
        [HttpPost("chat-messages/{flowId}")]
        [AllowAnonymous]
        public async Task ChatStream(long flowId, [FromBody] AIChatRequest request)
        {
            var flow = await _freeSql.Select<FlowEntity>().Where(x => x.Id == flowId).FirstAsync();
            if (flow == null)
            {
                throw new WebApiException("can not find flow");
            }

            // 设置 SSE 响应头
            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");
            Response.Headers.Append("X-Accel-Buffering", "no"); // 禁用 Nginx 缓冲

            FlowChatConversationEntity? conversation = null;

            if (string.IsNullOrEmpty(request.ConversationId))
            {
                // 遵从dify的设计思路, ConversationId为空时, 生成一个
                request.ConversationId = SnowflakeId.NextId().ToString();
            }
            else
            {
                // ConversationId不为空时, 从数据库中查询会话变量
                conversation = await _freeSql.Select<FlowChatConversationEntity>().Where(x => x.ConversationId == request.ConversationId).FirstAsync();
                if (conversation == null)
                {
                    throw new WebApiException("can not find conversation : " + request.ConversationId);
                }
            }

            FlowRuntimeAIContext runtimeAIContext = new FlowRuntimeAIContext()
            {
                FlowId = flowId,
                FlowInstanceId = SnowflakeId.NextId(),
                DisplayName = flow.DisplayName,
                FlowConfigInfoForRun = flow.ConfigInfoForRun,
                HttpContext = HttpContext,

                ServiceProvider = _serviceProvider,
                StartTime = DateTime.Now,
                User = request.User,
                InputVariables = AIChatRequest.BuildInputParameters(flow.ConfigInfoForRun, request.Inputs),
                InputVariablesOriginal = request.Inputs,

                Variables = flow.ConfigInfoForRun.Variables.ToList(),
                FreeSql = _freeSql,
                Request = request,
            };
            if (conversation != null)
            {
                // 还原会话变量
                foreach (var item in conversation.Variables)
                {
                    var variable = runtimeAIContext.Variables.FirstOrDefault(x => x.Id == item.Key);
                    if (variable != null)
                    {
                        variable.SetValue(item.Value, out string errors);
                    }
                }
            }

            var runtimeAIService = new FlowRuntimeAIService(_serviceProvider, _freeSql);

            runtimeAIService.StreamingOutputCallback = async (eventType, nodeId, data) =>
            {
                try
                {
                    object sseData;

                    if (eventType == "message")
                    {
                        // 流式消息块
                        sseData = new
                        {
                            @event = "message",
                            createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            answer = data.ToString(),
                            metadata = new
                            {
                                node_id = nodeId
                            }
                        };
                    }
                    else if (eventType == "node_started" || eventType == "node_finished")
                    {
                        sseData = new
                        {
                            @event = eventType,
                            createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            data = data
                        };
                    }
                    else
                    {
                        return; // 未知事件类型，忽略
                    }

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(sseData);
                    await Response.WriteAsync($"data: {json}\n\n");
                    await Response.Body.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"流式输出错误: {ex.Message}");
                }
            };

            try
            {
                // 发送开始事件
                await SendSseEvent("workflow_started", new
                {
                    conversationId = runtimeAIContext.Request.ConversationId,
                    flowInstanceId = runtimeAIContext.FlowInstanceId,
                    createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

                // 执行流程
                await runtimeAIService.RunAsync(runtimeAIContext);

                // 发送完成事件
                await SendSseEvent("workflow_finished", new
                {
                    conversationId = runtimeAIContext.Request.ConversationId,
                    flowInstanceId = runtimeAIContext.FlowInstanceId,
                    createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            }
            catch (Exception ex)
            {
                // 发送错误事件
                await SendSseEvent("error", new
                {
                    conversationId = runtimeAIContext.Request.ConversationId,
                    flowInstanceId = runtimeAIContext.FlowInstanceId,
                    error = ex.Message,
                    createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            }
            finally
            {
                // 发送结束标记
                await Response.WriteAsync("data: [DONE]\n\n");
                await Response.Body.FlushAsync();
            }
        }

        private async Task SendSseEvent(string eventType, object data)
        {
            try
            {
                var eventData = new
                {
                    @event = eventType,
                    data = data
                };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(eventData);
                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();
            }
            catch
            {
                // 忽略写入错误
            }
        }

    }
}
