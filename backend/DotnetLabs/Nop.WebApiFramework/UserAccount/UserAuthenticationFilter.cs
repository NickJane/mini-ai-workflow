using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.WebApiFramework.UserAccount.Jwt.Model;
using System.Linq;
using System.Security.Claims;

namespace Nop.WebApiFramework.UserAccount
{
    public class UserAuthenticationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 检查是否已通过认证
            if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                var identity = context.HttpContext.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    // 从 Claims 中提取用户信息
                    var userIdClaim = identity.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Id);
                    var nameClaim = identity.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Name);
                    var roleClaim = identity.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Role);

                    if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
                    {
                        // 创建 UserDto 对象
                        var userDto = new UserDto
                        {
                            Id = userId,
                            NickName = nameClaim?.Value ?? string.Empty,
                            Role = roleClaim?.Value ?? string.Empty
                        };

                        LoginUserDto.SetCurrent(context.HttpContext, userDto);
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
