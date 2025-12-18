using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Nop.WebApiFramework.UserAccount
{
    public class AuthTokenDto
    {
        [JsonProperty("type")]
        public string Type { get; set; } = JwtBearerDefaults.AuthenticationScheme.ToString();
        /// <summary>
        /// Token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 过期时间(时间戳秒数)
        /// </summary>
        [JsonProperty("expires")]
        public long ExpiresIn { get; set; }

        /// <summary>
        /// 刷新Token
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
