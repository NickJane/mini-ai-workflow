/**
 * 流程管理相关类型定义
 */

import type { NodeBase } from './flow-designer/NodeBase';
import type { NodeLine } from './flow-designer/NodeLine';
import type { AnyVariable } from './flow-designer/Parameters/Variable';

/**
 * 流程类型枚举
 * LogicFlow: 逻辑流程
 * AIFlow: AI流程
 * ApprovalFlow: 审批流程
 */
export enum FlowType {
  LogicFlow = 'LogicFlow',
  AIFlow = 'AIFlow',
  ApprovalFlow = 'ApprovalFlow'
}

/**
 * 流程配置信息
 * 包含流程的节点、连接线、变量和参数配置
 */
export interface IFlowConfigInfo {
  /**
   * 自定义变量列表
   */
  variables?: AnyVariable[] | null;
  
  /**
   * 输入参数列表
   */
  inputParameters?: AnyVariable[] | null;
  
  /**
   * 节点列表
   */
  nodes?: NodeBase[] | null;
  
  /**
   * 连接线列表
   */
  lines?: NodeLine[] | null;
}

/**
 * 流程DTO
 * 用于创建、更新和查询流程
 */
export interface IFlowDto {
  /** 流程ID，创建时为null */
  id?: number | null;
  /** 流程显示名称 */
  displayName?: string | null;
  /** 流程类型 */
  flowType?: FlowType;
  /** 流程描述 */
  description?: string | null;
  /** 运行配置信息 */
  configInfoForRun?: IFlowConfigInfo | null;
  /** Web配置信息（JSON字符串） */
  configInfoForWeb?: string | null;
  /** 最后修改时间 */
  lastModified?: string | null;
  /** 最后修改人 */
  lastModifyBy?: string | null;
}

/**
 * 创建流程请求参数
 */
export interface ICreateFlowRequest {
  /** 流程显示名称 */
  displayName: string;
  /** 流程类型 */
  flowType: FlowType;
  /** 流程描述 */
  description?: string;
  /** 运行配置信息 */
  configInfoForRun?: IFlowConfigInfo | null;
  /** Web配置信息 */
  configInfoForWeb?: string | null;
}

/**
 * 更新流程请求参数
 */
export interface IUpdateFlowRequest {
  /** 流程ID */
  id: number;
  /** 流程显示名称 */
  displayName?: string;
  /** 流程描述 */
  description?: string;
  /** 运行配置信息 */
  configInfoForRun?: IFlowConfigInfo | null;
  /** Web配置信息 */
  configInfoForWeb?: string | null;
}

/**
 * 获取流程列表请求参数
 */
export interface IGetFlowListRequest {
  /** 流程类型筛选，null表示获取所有类型 */
  flowType?: FlowType | null;
  /** 页码，从1开始 */
  pageIndex?: number;
  /** 每页数量 */
  pageSize?: number;
}

/**
 * 流程运行日志详情
 * 描述单个节点执行的记录
 */
export interface IFlowRunLogDetail {
  /** 日志详情ID */
  id: number;
  /** 运行日志主表ID */
  flowRunLogId: number;
  /** 节点ID */
  nodeId?: string | null;
  /** 节点名称 */
  nodeName?: string | null;
  /** 节点类型 */
  nodeType?: string | null;
  /** 是否执行成功 */
  isSuccess: boolean;
  /** 节点执行产生的文本日志 */
  logs?: string[] | null;
  /** 节点执行结果（结构化JSON） */
  nodeResult?: unknown;
  /** 日志记录创建时间 */
  createdTime: string;
}

/**
 * 流程运行日志
 * 记录整个流程一次运行的概要信息
 */
export interface IFlowRunLogDto {
  /** 日志ID/实例ID */
  flowInstanceId: number;
  /** 对应的流程ID */
  flowId: number;
  /** 流程显示名称 */
  displayName?: string | null;
  /** 流程类型 */
  flowType: FlowType;
  /** 节点运行详情 */
  details?: IFlowRunLogDetail[] | null;
  /** 运行开始时间 */
  createdTime: string;
  /** 运行结束时间 */
  finishedTime?: string | null;
  /** 运行耗时（毫秒） */
  runDurationMs?: number;
  /** 触发运行的用户 */
  runUser?: string | null;
  /** 运行是否成功 */
  isSuccess: boolean;
  aIChatRequest : AIChatRequest
}


    /**
 * AI 聊天请求模型
 */
export interface AIChatRequest {
  /** 对话内容 */
  query: string;
  
  /** 用户标识, 前端可不传, 后端自动获取 */
  user: string;
  
  /** 会话ID，用于继续特定会话; 参考dify的逻辑, 传空的话, 则创建新的会话 */
  conversationId?: string;
  
  /** 是否启用流式响应, blocking: 阻止模式, streaming: 流式模式 */
  responseMode?: string;
  
  /** 附加输入参数, 该参数对应入参 */
  inputs: Record<string, unknown>;
  
  /** 文件列表，支持上传图片、文档等文件 */
  files: AIFileRequest[];
}

/**
 * AI 请求文件模型, 仅支持在线文件
 */
export interface AIFileRequest {
  /** 文件URL */
  url: string;
  
  /** 文件名称 */
  name?: string;
  
  /** 文件大小（字节） */
  size?: number;
  
  /** 文件MIME类型 */
  mimeType?: string;
}
