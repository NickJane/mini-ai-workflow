using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.WebApiFramework.UserAccount
{
    /// <summary>
    /// jwt实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JwtToken<T>
    {
        [JsonProperty("iss")]
        public string Iss { get; set; }
        [JsonProperty("exp")]
        public long Exp { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
    }



    public class RefreshToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

}
