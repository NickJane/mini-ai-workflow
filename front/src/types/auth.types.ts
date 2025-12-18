/**
 * 认证相关类型定义
 */

/**
 * 登录/注册请求参数
 */
export interface ISignInRequest {
  /** 手机号码 */
  phoneNumber: string;
  /** 密码 */
  password: string;
}

/**
 * Token信息
 */
export interface ITokenInfo {
  /** Token类型，如 "Bearer" */
  type: string;
  /** 访问令牌 */
  accessToken: string;
  /** 过期时间戳 */
  expires: number;
  /** 刷新令牌 */
  refreshToken: string;
}

/**
 * 登录用户信息
 */
export type LoginUserInfo = {
  id: string;
  name: string;
  role: UserRole;
};

/**
 * 用户角色枚举
 */
export enum UserRole {
  Admin = 'Admin',
  User = 'User'
}

/**
 * 用户信息
 */
export type UserInfo = {
  id: number,
  phoneNumber: string
}
