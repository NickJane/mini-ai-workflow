/**
 * 认证状态管理
 * 使用 Pinia 的选项式 API，管理用户登录状态和token
 */
import { defineStore } from 'pinia';
import type { ITokenInfo } from '@/types/auth.types';
import { AuthService } from '@/services/auth.service';
import router from '@/router';

// 定义 Store 的状态接口
interface IAuthState {
  tokenInfo: ITokenInfo | null;
  refreshTimerId: number | null;
}

/**
 * 认证状态管理Store
 */
export const useAuthStore = defineStore('auth', {
  // 状态定义
  state: (): IAuthState => ({
    /** Token信息 */
    tokenInfo: JSON.parse(localStorage.getItem('tokenInfo') || 'null'),
    /** 刷新定时器ID */
    refreshTimerId: null,
  }),

  // 类似于计算属性
  getters: {
    /**
     * 是否已认证
     * @returns {boolean}
     */
    getIsAuthenticated: (state) => !!state.tokenInfo,

    /**
     * 获取访问令牌
     * @returns {string | null}
     */
    getAccessToken: (state) => state.tokenInfo?.accessToken ?? null,

    /**
     * 获取登录用户信息（从JWT token中解析）
     */
    getLoginUserInfo: (state) => {
      if (state.tokenInfo?.accessToken) {
        const payloadBase64 = state.tokenInfo?.accessToken.split('.')[1];
        const payloadJson = atob(payloadBase64);
        const payload = JSON.parse(payloadJson);
        return {
          id: payload.sub,
          name: payload.name,
          role: payload.role
        }
      }
      return null;
    },

    /**
     * 获取登录用户角色
     */
    getLoginUserRole: (state) => {
      if (state.tokenInfo?.accessToken) {
        const payloadBase64 = state.tokenInfo?.accessToken.split('.')[1];
        const payloadJson = atob(payloadBase64);
        const payload = JSON.parse(payloadJson);
        return payload.role;
      }
      return null;
    }
  },

  // 定义可以改变状态的同步或异步操作
  actions: {
    /**
     * 启动token刷新定时器
     * 私有方法，每9分钟自动刷新一次token
     */
    _startRefreshTimer() {
      if (this.refreshTimerId) {
        clearInterval(this.refreshTimerId);
      }
      // 每9分钟刷新一次token
      this.refreshTimerId = window.setInterval(async () => {
        await this.refreshToken();
      }, 9 * 60 * 1000);
    },

    /**
     * 登录
     * @param {string} phoneNumber - 手机号
     * @param {string} password - 密码
     * @returns {Promise<ITokenInfo | null>}
     */
    async signIn(phoneNumber: string, password: string) {
      const response = await AuthService.signIn({ phoneNumber, password });
      this.tokenInfo = response.data || null;
      // 持久化存储
      if (this.tokenInfo) {
        localStorage.setItem('tokenInfo', JSON.stringify(this.tokenInfo));
        localStorage.setItem('phoneNumber', phoneNumber);
      }
      this._startRefreshTimer();
      return response.data;
    },

    /**
     * 登出
     * 清除token和相关信息，跳转到登录页
     */
    signOut() {
      this.tokenInfo = null;
      // 清除持久化存储
      localStorage.removeItem('tokenInfo');
      if (this.refreshTimerId) {
        clearInterval(this.refreshTimerId);
        this.refreshTimerId = null;
      }
      router.push('/login');
    },

    /**
     * 初始化
     * 应用启动时调用，如果有token则启动刷新定时器
     */
    init() {
      // 如果有 token，启动刷新定时器
      if (this.tokenInfo) {
        this._startRefreshTimer();
      }
    },

    /**
     * 刷新token
     * 使用当前的refreshToken获取新的accessToken
     */
    async refreshToken() {
      if (this.tokenInfo) {  // 如果有 tokenInfo，就用整个 tokenInfo 对象去刷新
        try {
          const response = await AuthService.refreshToken(this.tokenInfo);
          this.tokenInfo = response.data || null;
          // 持久化存储
          if (this.tokenInfo) {
            localStorage.setItem('tokenInfo', JSON.stringify(this.tokenInfo));
          }
        } catch (error) {
          console.error('刷新token失败', error);
          // 刷新失败可能需要登出
          this.signOut();
        }
      }
    }
  }
});
