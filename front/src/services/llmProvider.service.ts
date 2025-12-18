/**
 * 大模型提供者管理服务
 * 提供模型提供者的增删改查等API调用
 */
import type { TApiResponse } from '@/types/ApiResponse{T}';
import type { 
  IFlowLLMProviderDto, 
  IGetLLMProviderListParams 
} from '@/types/llmProvider.types';
import { HttpUtil } from '@/utils/http.util';

export class LLMProviderService {
  /**
   * 创建大模型提供者
   * @param data - 大模型提供者信息
   * @returns Promise<TApiResponse<IFlowLLMProviderDto>>
   */
  public static async createProvider(data: IFlowLLMProviderDto): Promise<TApiResponse<IFlowLLMProviderDto>> {
    const response = await HttpUtil.getInstance().post<TApiResponse<IFlowLLMProviderDto>>('/FlowLLMProvider/Create', data);
    return response.data;
  }

  /**
   * 根据ID获取大模型提供者信息
   * @param id - 大模型提供者ID
   * @returns Promise<TApiResponse<IFlowLLMProviderDto>>
   */
  public static async getProviderById(id: number): Promise<TApiResponse<IFlowLLMProviderDto>> {
    const response = await HttpUtil.getInstance().get<TApiResponse<IFlowLLMProviderDto>>('/FlowLLMProvider/GetById', {
      params: { id }
    });
    return response.data;
  }

  /**
   * 获取大模型提供者列表
   * 支持按平台名称筛选
   * @param params - 查询参数
   * @returns Promise<TApiResponse<IFlowLLMProviderDto[]>>
   */
  public static async getProviderList(params?: IGetLLMProviderListParams): Promise<TApiResponse<IFlowLLMProviderDto[]>> {
    const response = await HttpUtil.getInstance().get<TApiResponse<IFlowLLMProviderDto[]>>('/FlowLLMProvider/GetList', {
      params: {
        platformName: params?.platformName
      }
    });
    return response.data;
  }

  /**
   * 更新大模型提供者信息
   * @param data - 更新的大模型提供者信息
   * @returns Promise<TApiResponse<IFlowLLMProviderDto>>
   */
  public static async updateProvider(data: IFlowLLMProviderDto): Promise<TApiResponse<IFlowLLMProviderDto>> {
    const response = await HttpUtil.getInstance().post<TApiResponse<IFlowLLMProviderDto>>('/FlowLLMProvider/Update', data);
    return response.data;
  }

  /**
   * 删除大模型提供者
   * 物理删除，不可恢复
   * @param id - 大模型提供者ID
   * @returns Promise<TApiResponse<boolean>>
   */
  public static async deleteProvider(id: number): Promise<TApiResponse<boolean>> {
    const response = await HttpUtil.getInstance().post<TApiResponse<boolean>>('/FlowLLMProvider/Delete', null, {
      params: { id }
    });
    return response.data;
  }
}
