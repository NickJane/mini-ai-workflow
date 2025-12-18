using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Nop.Infrastructure.Json
{
    public sealed class ConverterContractResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly JsonConverter[] converters;

        public ConverterContractResolver(params JsonConverter[] converters)
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



    public class UppercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToUpper();
        }
    }

    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}
