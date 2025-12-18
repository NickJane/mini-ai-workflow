using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Nop.Infrastructure.Json
{
    public sealed class CustomConverterContractResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly JsonConverter[] converters;
        /// <summary>
        /// 继承自DefaultContractResolver
        /// </summary>
        /// <param name="converters"></param>
        public CustomConverterContractResolver(params JsonConverter[] converters)
        {
            this.converters = converters;
        }

        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
        }
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            var result = base.ResolveContractConverter(objectType);

            if (result != null)
            {
                return result;
            }

            foreach (var converter in converters)
            {
                if (converter.CanConvert(objectType))
                {
                    return converter;
                }
            }

            return null;
        }
    }
}
