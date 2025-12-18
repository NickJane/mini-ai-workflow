/**
 * 流程管理服务
 * 提供流程的增删改查等API调用
 */
import type { TApiResponse } from '@/types/ApiResponse{T}';
import type { 
  IFlowDto, 
  ICreateFlowRequest, 
  IUpdateFlowRequest, 
  IGetFlowListRequest,
  IFlowRunLogDto
} from '@/types/flow.types';
import { HttpUtil } from '@/utils/http.util';

export class FlowService {
  /**
   * 创建新的流程
   * 支持LogicFlow/AIFlow/ApprovalFlow三种类型
   * @param data - 创建流程的请求参数
   * @returns Promise<TApiResponse<IFlowDto>>
   */
  public static async createFlow(data: ICreateFlowRequest): Promise<TApiResponse<IFlowDto>> {
    const response = await HttpUtil.getInstance().post<TApiResponse<IFlowDto>>('/Flow/Create', data);
    return response.data;
  }

  /**
   * 根据ID获取流程信息
   * @param id - 流程ID
   * @returns Promise<TApiResponse<IFlowDto>>
   */
  public static async getFlowById(id: number): Promise<TApiResponse<IFlowDto>> {
    const response = await HttpUtil.getInstance().get<TApiResponse<IFlowDto>>('/Flow/GetById', {
      params: { id }
    });
    return response.data;
  }

  /**
   * 获取流程列表
   * 支持按类型筛选和分页
   * @param params - 查询参数
   * @returns Promise<TApiResponse<IFlowDto[]>>
   */
  public static async getFlowList(params?: IGetFlowListRequest): Promise<TApiResponse<IFlowDto[]>> {
    const response = await HttpUtil.getInstance().get<TApiResponse<IFlowDto[]>>('/Flow/GetList', {
      params: {
        flowType: params?.flowType,
        pageIndex: params?.pageIndex ?? 1,
        pageSize: params?.pageSize ?? 20
      }
    });
    return response.data;
  }

  /**
   * 更新流程信息
   * 只更新提供的字段，null值的字段不更新
   * @param data - 更新流程的请求参数
   * @returns Promise<TApiResponse<IFlowDto>>
   */
  public static async updateFlow(data: IUpdateFlowRequest): Promise<TApiResponse<IFlowDto>> {
    const response = await HttpUtil.getInstance().post<TApiResponse<IFlowDto>>('/Flow/Update', data);
    return response.data;
  }

  /**
   * 删除流程
   * 物理删除，不可恢复
   * @param id - 流程ID
   * @returns Promise<TApiResponse<boolean>>
   */
  public static async deleteFlow(id: number): Promise<TApiResponse<boolean>> {
    const response = await HttpUtil.getInstance().post<TApiResponse<boolean>>('/Flow/Delete', null, {
      params: { id }
    });
    return response.data;
  }

  /**
   * 根据流程ID获取运行日志列表
   * @param flowId - 流程ID
   * @returns Promise<TApiResponse<IFlowRunLogDto[]>>
   */
  public static async getExecutionLogsByFlowId(flowId: number): Promise<TApiResponse<IFlowRunLogDto[]>> {
    const response = await HttpUtil.getInstance().get<TApiResponse<IFlowRunLogDto[]>>('/Flow/GetExecutionLogsByFlowId', {
      params: { flowId }
    });
    return response.data;
  }
}
