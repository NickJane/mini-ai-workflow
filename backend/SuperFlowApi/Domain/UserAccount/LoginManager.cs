using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.Extensions.Options;
using Nop.WebApiFramework.configuration;
using Nop.WebApiFramework.OpenTelemetry;

namespace SuperFlowApi.Domain.UserAccount
{
    public class LoginManager
    {
        private JwtSettingModel _jwtSetting;
        private JwtTokenProvider _jwtTokenProvider;
        private HttpContext _httpContext;

        public LoginManager(
        IOptions<AppSettingsModel> options,
        JwtTokenProvider _jwtTokenProvider,
        IHttpContextAccessor httpContextAccessor)
        {
            _jwtSetting = options.Value.JwtSetting;
            this._jwtTokenProvider = _jwtTokenProvider;
            _httpContext = httpContextAccessor.HttpContext;
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthTokenDto> Login(UserDto dto)
        {
            var token = await _jwtTokenProvider.CreateAuthTokenDto(dto);
            return token;
        }

        public async Task<AuthTokenDto> RefreshToken(AuthTokenDto request, Func<Task<UserDto>> fnGetUser)
        {
            var token = await _jwtTokenProvider.RefreshAuthTokenAsync(request, fnGetUser);
            return token;
        }



    }
}