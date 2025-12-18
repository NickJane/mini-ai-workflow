using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nop.WebApiFramework.UserAccount
{
    /// <summary>
    /// 保存登录用户信息
    /// 通过 AsyncLocal 在同一执行上下文中传递用户信息（Filter → Controller）
    /// </summary>
    public static class LoginUserDto
    {
        private static readonly AsyncLocal<UserDto> _userCurrent = new();

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        public static UserDto Current => _userCurrent.Value;

        /// <summary>
        /// 设置当前用户信息（由 UserAuthenticationFilter 调用）
        /// </summary>
        internal static void SetCurrent(HttpContext httpContext, UserDto user)
        {
            _userCurrent.Value = user;
        }

        /// <summary>
        /// 清除当前用户信息
        /// </summary>
        internal static void Clear()
        {
            _userCurrent.Value = null;
        }
    }

    public class UserDto
    {
        public virtual long Id { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public virtual string PhoneNumber { get; set; }

        public string Password { get; set; }

        public virtual string NickName { get; set; }

        /// <summary>
        /// 最后登录IP
        /// </summary>
        public virtual string LastLoginIp { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public virtual DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public virtual DateTime RegisterTime { get; set; }


        /// <summary>
        /// 注册来源码
        /// </summary>
        /// <value></value>
        public virtual string? Source { get; set; }

        /// <summary>
        /// 删除
        /// </summary>
        /// <value></value>
        public virtual bool IsDeleted { get; set; }

        /// <summary>
        /// 账号是否启用
        /// </summary>
        public bool? IsEnable { get; set; } = true;

        /// <summary>
        /// 锁定到期时间
        /// </summary>
        /// <value></value>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        /// 是否锁定
        /// </summary>
        public virtual bool LockoutEnabled { get; set; } = false;

        /// <summary>
        /// 失败次数
        /// </summary>
        public virtual int? AccessFailedCount { get; set; }

        public string Role { get; set; }

    }
}