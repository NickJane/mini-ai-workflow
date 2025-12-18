using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nop.WebApiFramework.configuration;
using Nop.WebApiFramework.UserAccount.Jwt.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Nop.WebApiFramework.UserAccount
{

    public class JwtTokenProvider
    {
        private const string RefreshTokenIdClaimType = "refresh_token_id";
        private readonly JwtSettingModel _jwtSetting;
        private readonly JwtBearerOptions _jwtBearerOptions;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenProvider(
            IOptions<AppSettingsModel> options,
        IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions,
        SigningCredentials signingCredentials)
        {
            _jwtBearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
            _signingCredentials = signingCredentials;
            _jwtSetting = options.Value.JwtSetting;
        }
        public async Task<AuthTokenDto> CreateAuthTokenDto(UserDto user)
        {
            AuthTokenDto dto = new AuthTokenDto();
            // 先创建refresh token
            var _refreshTuple = await this.CreateRefreshTokenAsync(user.Id.ToString());
            dto.RefreshToken = _refreshTuple.refreshToken;
            // 再签发Jwt
            var _tuple = this.CreateAccessToken(user, _refreshTuple.refreshTokenId);
            dto.AccessToken = _tuple.Item1;
            dto.ExpiresIn = _tuple.Item2;

            return dto;
        }

        public async Task<AuthTokenDto> RefreshAuthTokenAsync(AuthTokenDto token, Func<Task<UserDto>> fnGetUser)
        {

            var validationParameters = _jwtBearerOptions.TokenValidationParameters.Clone();
            // 不校验生命周期
            validationParameters.ValidateLifetime = false;

            var handler = _jwtBearerOptions.TokenHandlers.OfType<JwtSecurityTokenHandler>().FirstOrDefault()
                ?? new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = null;
            try
            {
                principal = handler.ValidateToken(token.AccessToken, validationParameters, out _);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid access token");
            }

            var identity = principal.Identities.First();
            var userId = identity.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Id).Value;
            var refreshTokenId = identity.Claims.FirstOrDefault(c => c.Type == RefreshTokenIdClaimType).Value;
            var refreshTokenKey = GetRefreshTokenKey(userId, refreshTokenId);

            var user = await fnGetUser();

            return await CreateAuthTokenDto(user);
        }


        private (string, long) CreateAccessToken(UserDto user, string refresh_token_id)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim> {
                    new Claim(JwtClaimTypes.Id, user.Id.ToString()),
                    new Claim(JwtClaimTypes.Name, user.NickName),
                    new Claim(JwtClaimTypes.Role, user.Role),
                    // 刷新token的id
                    new Claim(RefreshTokenIdClaimType, refresh_token_id)
                }),
                Issuer = _jwtSetting.Issuer,
                Audience = _jwtSetting.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSetting.ExpireSeconds / 60),
                SigningCredentials = _signingCredentials
            };
            var handler = _jwtBearerOptions.TokenHandlers.OfType<JwtSecurityTokenHandler>().FirstOrDefault()
                ?? new JwtSecurityTokenHandler();
            var securityToken = handler.CreateJwtSecurityToken(tokenDescriptor);
            var token = handler.WriteToken(securityToken);

            DateTimeOffset dateTimeOffset = new DateTimeOffset(tokenDescriptor.Expires.Value);
            return (token, dateTimeOffset.ToUnixTimeSeconds());
        }
        private Task<(string refreshTokenId, string refreshToken)> CreateRefreshTokenAsync(string userId)
        {
            var tokenId = Guid.NewGuid().ToString("N");

            var rnBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(rnBytes);
            var token = Convert.ToBase64String(rnBytes);


            return Task.FromResult((tokenId, token));
        }
        private string GetRefreshTokenKey(string userId, string refreshTokenId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(refreshTokenId)) throw new ArgumentNullException(nameof(refreshTokenId));

            return $"{userId}:{refreshTokenId}";
        }


    }
}
