using System;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nop.Infrastructure.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace Nop.Infrastructure
{
    public static class SerializationServices
    {
        private static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings, List<JsonConverter> customJsonConverters = null)
        {
            List<JsonConverter> lstJsonConverter = new List<JsonConverter>()
            {
                //new BoolConvert(),
                new JsonSchemaTypeConvert(),
                new StringEnumConverter()
            };
            if (customJsonConverters != null)
                lstJsonConverter.AddRange(customJsonConverters);

         
            settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.None;

            settings.ContractResolver = new CustomConverterContractResolver(lstJsonConverter.ToArray());

            return settings;
        }

        /// <summary>
        /// 注入入口
        /// </summary>
        /// <param name="settings">全局的newtonsoft序列化配置类</param>
        /// <param name="customJsonConverters">实现了jsonConvert的自定义序列化类</param>
        public static void SetJsonSerializerSettings(JsonSerializerSettings settings, List<JsonConverter> customJsonConverters = null)
        {
            ConfigureJson(settings, customJsonConverters);

            JsonConvert.DefaultSettings = () => { return settings; };
        }
    }
}
