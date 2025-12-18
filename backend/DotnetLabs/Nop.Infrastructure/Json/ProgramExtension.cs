using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Nop.Infrastructure.Json
{
    public static class ProgramExtension
    {
        public static void AddCustomNewtonsoftSupport(this IMvcBuilder builder)
        {
            builder.AddNewtonsoftJson(x =>
            {
                SerializationServices.SetJsonSerializerSettings(
                    x.SerializerSettings,
                    new List<JsonConverter> {
                        new StringEnumConverter(),
                        // 不考虑国际时间了.
                        // new DateTimeJsonConverter() 
                    }
                );

                x.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });
        }
    }
}