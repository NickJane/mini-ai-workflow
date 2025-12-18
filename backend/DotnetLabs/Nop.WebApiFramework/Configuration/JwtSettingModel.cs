
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.WebApiFramework.configuration
{
    public class JwtSettingModel
    {


        public string SecurityKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int ExpireSeconds { get; set; }
        public int RefreshExpiresSeconds { get; set; }

        [JsonIgnore]
        public SymmetricSecurityKey SymmetricSecurityKey => new(Encoding.UTF8.GetBytes(SecurityKey));
    }
}
