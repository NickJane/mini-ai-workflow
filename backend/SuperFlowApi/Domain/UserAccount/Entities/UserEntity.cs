using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.UserAccount
{
    [Table(Name = "UserEntity")]
    [Index("idx_PhoneNumber", "PhoneNumber", IsUnique = true)]
    public class UserEntity
    {
        [Column(IsPrimary = true)]
        public virtual long Id { get; set; } = default!;

        /// <summary>
        /// 手机号码
        /// </summary>
        [Column(StringLength = 50)]
        public virtual string PhoneNumber { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }


        /// <summary>
        /// 昵称
        /// </summary>
        [Column(StringLength = 100)]
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

        public UserRole Role { get; set; } = UserRole.User;


    }

    public enum UserRole
    {
        Admin,
        User,
    }
}