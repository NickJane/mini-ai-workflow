using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SuperFlowApi.Domain.UserAccount;
using Nop.WebApiFramework.Exceptions;
using Nop.WebApiFramework.UserAccount.Jwt.Model;
using Nop.WebApiFramework.UserAccount;

namespace SuperFlowApi.Controllers
{
    [ApiGroup(nameof(EnumApiGroupNames.UserAccount))]
    public class UserAccountController : BaseControllerController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserService _userService;
        private readonly LoginManager _loginManager;

        public UserAccountController(IHttpClientFactory _httpClientFactory, UserService _userService, LoginManager _loginManager)
        {
            this._httpClientFactory = _httpClientFactory;
            this._userService = _userService;
            this._loginManager = _loginManager;
        }

        /// <summary>
        /// 用户注册或登录
        /// </summary>
        /// <param name="requestObj">用户信息</param>
        /// <remarks>
        /// 请求示例:
        /// POST /RegistOrLogin
        /// {
        ///    "username": "string",
        ///    "password": "string"
        /// }
        /// </remarks>
        /// <returns>返回用户认证信息</returns>
        [HttpPost("RegistOrLogin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(JsonResponse<AuthTokenDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegistOrLoginAsync([FromBody] UserDto requestObj)
        {
            var user = await _userService.RegistOrLoginAsync(requestObj.PhoneNumber, requestObj.Password, HttpContext);

            var dto = JsonConvert.DeserializeObject<UserDto>(JsonConvert.SerializeObject(user));
            if (user != null)
            {
                var token = await _loginManager.Login(dto);
                return Ok(token);
            }
            throw new WebApiException("user not found");
        }

        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("refreshtoken")]
        [ProducesResponseType(typeof(JsonResponse<AuthTokenDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Refreshtoken([FromBody] AuthTokenDto dto)
        {
            var userid = HttpContext.User.Claims.First(x => x.Type == JwtClaimTypes.Id);
            var res = await _loginManager.RefreshToken(dto, async () =>
             {
                 var user = await _userService.GetUserById(long.Parse(userid.Value));
                 if (user == null)
                     throw new WebApiException("user not found");

                 var dto = JsonConvert.DeserializeObject<UserDto>(JsonConvert.SerializeObject(user));
                 return dto;
             });

            return Ok(res);
        }


        /// <summary>
        /// GetUserById
        /// </summary>
        /// <returns></returns>
        [Route("GetUserById")]
        [HttpGet]
        public async Task<IActionResult> GetUserByIdAsync([FromQuery] long id)
        {
            var user = await _userService.GetUserById(id);
            if (user != null)
                return Ok(user);
            else
            {
                return Error("user not found");
            }
        }

        [Route("GetUsers")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var user = await _userService.GetUsers();
            if (user != null)
                return Ok(user);
            else
            {
                return Error("user not found");
            }
        }

        [Route("GetCurrentUser")]
        [HttpGet]
        [ProducesResponseType(typeof(JsonResponse<UserDto>), (int)HttpStatusCode.OK)]
        public IActionResult GetCurrentUser()
        {
            var currentUser = LoginUserDto.Current;

            if (currentUser == null)
            {
                return Error("user not found");
            }

            return Ok(new
            {
                UserId = currentUser.Id,
                NickName = currentUser.NickName,
                Role = currentUser.Role,
                Message = "通过 LoginUserDto.Current 获取（由 UserAuthenticationFilter 自动设置）"
            });
        }
    }
}
