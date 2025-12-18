/**
 * 大模型提供者 DTO
 * 用于管理不同平台（如阿里云、OpenAI等）的大模型配置信息
 */
export interface IFlowLLMProviderDto {
  /** 主键ID */
  id?: number;
  
  /** 平台名称（如：阿里云、OpenAI等） */
  platformName?: string;
  
  /** 模型名称列表（如：["gpt-4", "gpt-4-turbo", "gpt-3.5-turbo"]），注意模型名称在平台内必须唯一 */
  llmNames?: string[];
  
  /** 大模型API地址 */
  llmapiUrl?: string;
  
  /** 大模型API密钥 */
  llmapiKey?: string;
}

/**
 * 大模型提供者列表请求参数
 */
export interface IGetLLMProviderListParams {
  /** 平台名称筛选（可选），支持模糊查询 */
  platformName?: string;
}
