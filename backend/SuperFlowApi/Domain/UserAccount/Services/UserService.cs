using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.UserAccount
{
    public class UserService
    {
        private readonly IFreeSql _freeSql;

        public UserService(FreeSqlProvider freeSqlProvider)
        {
            _freeSql = freeSqlProvider.GetFreeSql();
        }

        public async Task<UserEntity> RegistOrLoginAsync(string phoneNumber, string password, HttpContext httpContext)
        {
            var user = await _freeSql.Select<UserEntity>().Where(x => x.PhoneNumber == phoneNumber).FirstAsync();
            if (user == null)
            {
                var salt = Guid.NewGuid().ToString("N");
                user = new UserEntity
                {
                    Id = SnowflakeId.NextId(),
                    PhoneNumber = phoneNumber,
                    PasswordSalt = salt,
                    PasswordHash = (password + salt).ToMD5Token(),
                    NickName = phoneNumber,
                    RegisterTime = DateTime.UtcNow,
                    LastLoginIp = httpContext.GetClientIp(),
                    LastLoginTime = DateTime.UtcNow
                };
                await _freeSql.GetRepository<UserEntity>().InsertAsync(user);
            }
            else
            {
                user.LastLoginIp = httpContext.GetClientIp();
                user.LastLoginTime = DateTime.UtcNow;

                await _freeSql.GetRepository<UserEntity>().UpdateAsync(user);
            }
            return user;
        }

        public async Task<UserEntity> GetUserById(long id)
        {
            return await _freeSql.Select<UserEntity>().Where(x => x.Id == id).FirstAsync();
        }

        public async Task<List<UserEntity>> GetUsers()
        {
            return await _freeSql.Select<UserEntity>().Where(x => x.Role == UserRole.User).ToListAsync(x => new UserEntity
            {
                Id = x.Id,
                PhoneNumber = x.PhoneNumber
            });
        }
    }
}