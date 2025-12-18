/**
 * 认证服务
 * 提供登录、注册、刷新token等认证相关的API调用
 */
import type { TApiResponse } from '@/types/ApiResponse{T}';
import type { ISignInRequest, ITokenInfo, UserInfo } from '@/types/auth.types';
import { HttpUtil } from '@/utils/http.util';

export class AuthService {
  /**
   * 登录或注册
   * @param data - 登录请求参数（手机号和密码）
   * @returns Promise<TApiResponse<ITokenInfo>>
   */
  public static async signIn(data: ISignInRequest): Promise<TApiResponse<ITokenInfo>> {
    const response = await HttpUtil.getInstance().post<TApiResponse<ITokenInfo>>('/UserAccount/RegistOrLogin', data);
    return response.data;
  }

  /**
   * 刷新token
   * @param tokenInfo - 当前的token信息
   * @returns Promise<TApiResponse<ITokenInfo>>
   */
  public static async refreshToken(tokenInfo: ITokenInfo): Promise<TApiResponse<ITokenInfo>> {
    const response = await HttpUtil.getInstance().post<TApiResponse<ITokenInfo>>('/UserAccount/refreshtoken', tokenInfo);
    return response.data;
  }

  /**
   * 获取用户列表
   * @returns Promise<TApiResponse<UserInfo[]>>
   */
  public static async getUserInfoList(): Promise<TApiResponse<UserInfo[]>> {
    const response = await HttpUtil.getInstance().get<TApiResponse<UserInfo[]>>('/UserAccount/GetUsers');
    return response.data;
  }
}
