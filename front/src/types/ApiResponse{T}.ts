/**
 * 通用API响应类型
 * @template T - 响应数据的类型
 */
export type TApiResponse<T = any> = {
  /** 错误码，0表示成功 */
  errCode: number;
  /** 响应数据 */
  data?: T;
  /** 错误信息 */
  errMsg: string;
  /** 请求ID，用于追踪 */
  requestId: string;
}

/**
 * 分页数据类型
 * @template T - 分页项的类型
 */
export interface Paged<T = any> {
  items: T[]
  total: number
}
