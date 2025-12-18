using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nop.WebApiFramework.configuration;
using Nop.WebApiFramework.UserAccount.Jwt.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Nop.WebApiFramework.UserAccount
{
    public static class JwtExtentions
    {
        public static void GenerateRsaKeyParies(IWebHostEnvironment env)
        {
            RSAParameters privateKey, publicKey;

            // >= 2048 否则长度太短不安全
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    privateKey = rsa.ExportParameters(true);
                    publicKey = rsa.ExportParameters(false);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }

            var dir = Path.Combine(env.ContentRootPath, "Permissions/JwtRsa");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            System.IO.File.WriteAllText(Path.Combine(dir, "key.private.json"), JsonConvert.SerializeObject(privateKey));
            System.IO.File.WriteAllText(Path.Combine(dir, "key.public.json"), JsonConvert.SerializeObject(publicKey));
        }

        public static void AddJwtAuth(this IServiceCollection services, ConfigurationManager configuration)
        {
            var jwtSetting = configuration.GetSection("JwtSetting").Get<JwtSettingModel>();
            services.AddSingleton<JwtSettingModel>(x => { return jwtSetting; });

            services.AddSingleton(sp => new SigningCredentials(jwtSetting.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature));

            services.AddScoped<AppJwtBearerEvents>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    //是和token验证有关的参数配置，进行token验证时需要用到
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // TokenValidationParameters.ValidAlgorithms：有效的签名算法列表，即验证Jwt的Header部分的alg。默认为null，即所有算法均可。
                        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256, SecurityAlgorithms.RsaSha256 },
                        // TokenValidationParameters.ValidTypes：有效的token类型列表，即验证Jwt的Header部分的typ。默认为null，即算有算法均可。
                        ValidTypes = new[] { JwtConstants.HeaderType },
                        // TokenValidationParameters.ValidIssuer：有效的签发者，即验证Jwt的Payload部分的iss。默认为null。
                        // TokenValidationParameters.ValidIssuers：有效的签发者列表，可以指定多个签发者。
                        ValidIssuer = jwtSetting.Issuer,
                        // 是否验证签发者。默认为true。注意，如果设置了TokenValidationParameters.IssuerValidator，则该参数无论是何值，都会执行。
                        ValidateIssuer = true,
                        /*
                        TokenValidationParameters.ValidAudience：有效的受众，即验证Jwt的Payload部分的aud。默认为null。
                        TokenValidationParameters.ValidAudiences：有效的受众列表，可以指定多个受众。
                        */
                        ValidAudience = jwtSetting.Audience,
                        // 是否验证受众。默认为true。注意，如果设置了TokenValidationParameters.AudienceValidator，则该参数无论是何值，都会执行。
                        ValidateAudience = true,

                        IssuerSigningKey = jwtSetting.SymmetricSecurityKey,
                        // 用于验证Jwt签名的密钥。对于对称加密来说，加签和验签都是使用的同一个密钥；对于非对称加密来说，使用私钥加签，然后使用公钥验签。
                        ValidateIssuerSigningKey = true,
                        // 是否验证token是否在有效期内，即验证Jwt的Payload部分的nbf和exp。
                        ValidateLifetime = true,
                        // 是否要求token必须进行签名。默认为true，即token必须签名才可能有效。
                        RequireSignedTokens = true,
                        // 是否要求token必须包含过期时间。默认为true，即Jwt的Payload部分必须包含exp且具有有效值。
                        RequireExpirationTime = true,

                        /*
                        使用identityModel.JwtClaimTypes会比默认的ClaimName短很多
                        设置 HttpContext.User.Identity.NameClaimType，便于 HttpContext.User.Identity.Name 取到正确的值
                        设置 HttpContext.User.Identity.RoleClaimType，便于 HttpContext.User.Identity.IsInRole(xxx) 取到正确的值
                        */
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,

                        /*
                        设置时钟漂移，可以在验证token有效期时，允许一定的时间误差（如时间刚达到token中exp，但是允许未来5分钟内该token仍然有效）。默认为300s，即5min。
                        本例jwt的签发和验证均是同一台服务器，所以这里就不需要设置时钟漂移了。
                        */
                        ClockSkew = TimeSpan.Zero,
                    };

                    options.SaveToken = true;

                    options.SecurityTokenValidators.Clear();
                    // token生成器, 在JwtTokenProvider中将会调用他生成token
                    options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());

                    options.EventsType = typeof(AppJwtBearerEvents);
                });
        }
    }
}
