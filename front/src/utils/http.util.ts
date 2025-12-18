/**
 * HTTP 客户端工具
 * 基于 Axios 封装，提供统一的请求/响应拦截器
 */
import axios, { type AxiosInstance, type AxiosError } from 'axios';
import type { TApiResponse } from '@/types/ApiResponse{T}';
import { MessageUtil } from './message.util';
import router from '@/router';
import { getCurrentLanguage } from '@/locales';

/**
 * API错误类，用于统一处理业务错误
 */
export class ApiError extends Error {
  constructor(
    /** 错误码 */
    public errCode: number,
    /** 错误信息 */
    public errMsg: string,
    /** 请求ID */
    public requestId: string
  ) {
    super(errMsg);
    this.name = 'ApiError';
  }
}

/**
 * HTTP 客户端工具类
 * 实现单例模式，支持多个不同的 baseURL
 */
export class HttpUtil {
  private static instanceDic: Record<string, any> = {};

  /**
   * 获取 Axios 实例
   * @param baseURL - API基础URL，默认从环境变量读取
   * @returns Axios实例
   */
  public static getInstance(baseURL: string = import.meta.env.VITE_API_BASE_URL): AxiosInstance {
    if (HttpUtil.instanceDic[baseURL]) return HttpUtil.instanceDic[baseURL];
    else {
      var instance = HttpUtil.getInstanceInner(baseURL);
      HttpUtil.instanceDic[baseURL] = instance;
      return instance;
    }
  }

  /**
   * 创建新的 Axios 实例
   * @param baseURL - API基础URL
   * @returns 配置好的Axios实例
   */
  private static getInstanceInner(baseURL: string = import.meta.env.VITE_API_BASE_URL): AxiosInstance {

    var newInstance = axios.create({
      baseURL: baseURL,
    });

    // 请求拦截器 - 添加认证token和语言信息
    newInstance.interceptors.request.use(
      (config) => {
        const tokenInfo = JSON.parse(localStorage.getItem('tokenInfo') || 'null');
        if (tokenInfo) {
          // 设置Authorization请求头
          config.headers.Authorization = `${tokenInfo.type} ${tokenInfo.accessToken}`;
          // 可选：添加手机号到请求头
          config.headers["phoneNumber"] = localStorage.getItem('phoneNumber') || ''
        }
        
        // 添加语言信息到请求头
        const currentLanguage = getCurrentLanguage();
        config.headers['Accept-Language'] = currentLanguage;
        config.headers['X-Language'] = currentLanguage;
        
        return config;
      },
      (error) => Promise.reject(error)
    );

    // 响应拦截器 - 统一处理响应和错误
    newInstance.interceptors.response.use(
      (response) => {
        const apiResponse = response.data as TApiResponse<any>;
        // 业务错误处理
        if (apiResponse.errCode !== 0) {
          MessageUtil.error(apiResponse.errMsg);
          throw new ApiError(
            apiResponse.errCode,
            apiResponse.errMsg,
            apiResponse.requestId
          );
        }
        return response;
      },
      (error: AxiosError<TApiResponse<any>>) => {
        // HTTP错误处理
        if (error.response?.data) {
          const apiResponse = error.response.data;
          MessageUtil.error(apiResponse.errMsg);
          throw new ApiError(
            apiResponse.errCode,
            apiResponse.errMsg,
            apiResponse.requestId
          );
        }
        // 401未授权 - 跳转到登录页
        if (error.status === 401) {
          MessageUtil.error('未登录，请先登录');
          router.push('/login');
          throw error;
        }
        MessageUtil.error('网络请求失败，请稍后重试');
        throw error;
      }
    );

    return newInstance;
  }
}
