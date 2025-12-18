/**
 * 装饰器工具
 * 提供异常处理和加载状态管理的高阶函数
 */
import { MessageUtil } from './message.util'
import { ApiError } from '@/utils/http.util'
import type { TApiResponse } from '@/types/ApiResponse{T}'

/**
 * 异常处理和加载状态的高阶函数
 * @template T - 返回值类型
 * @param {() => Promise<T>} action - 要执行的异步操作
 * @param {{ value: boolean }} loading - 控制加载状态的 ref 对象
 * @param {string} [successMsg] - 可选的成功提示消息
 * @returns {Promise<T>} 返回原始操作的结果
 * @throws {ApiError} 当发生业务错误时抛出
 * @example
 * ```typescript
 * const handleSubmit = async () => {
 *   await withLoading(
 *     async () => {
 *       const response = await api.submit(data);
 *       return response;
 *     },
 *     loading,
 *     "操作成功"
 *   );
 * };
 * ```
 */
export const withLoading = async <T>(
  action: () => Promise<T>,
  loading: { value: boolean },
  successMsg?: string
): Promise<T> => {
  try {
    loading.value = true
    const result = await action()
    if (successMsg) {
      MessageUtil.handleResponse({
        errCode: 0,
        errMsg: successMsg,
        requestId: '',
        data: null
      } as TApiResponse<null>, successMsg)
    }
    return result
  } catch (err) {
    if (err instanceof ApiError) {
      MessageUtil.error(err.errMsg)
    } else {
      MessageUtil.error('操作失败，请稍后重试')
    }
    throw err
  } finally {
    loading.value = false
  }
}
