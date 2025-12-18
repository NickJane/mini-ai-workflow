/**
 * 统一消息提示工具
 * 封装 Element Plus 的消息组件，提供统一的API
 */
import { ElMessage } from 'element-plus'
import type { TApiResponse } from '@/types/ApiResponse{T}'

export const MessageUtil = {
  /**
   * 显示成功提示消息
   * @param {string} message - 要显示的消息内容
   */
  success(message: string) {
    ElMessage({
      message,
      type: 'success',
      duration: 3000
    })
  },

  /**
   * 显示错误提示消息
   * @param {string} message - 要显示的错误消息
   */
  error(message: string) {
    ElMessage({
      message,
      type: 'error',
      duration: 5000
    })
  },

  /**
   * 处理API响应，如果成功则显示成功消息
   * @template T - 响应数据类型
   * @param {TApiResponse<T>} response - API响应对象
   * @param {string} [successMsg] - 成功时显示的消息
   */
  handleResponse<T>(response: TApiResponse<T>, successMsg?: string) {
    if (response.errCode === 0 && successMsg) {
      this.success(successMsg)
    }
  }
}
