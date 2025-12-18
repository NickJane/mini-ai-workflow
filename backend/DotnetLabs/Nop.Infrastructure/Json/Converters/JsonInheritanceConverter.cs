using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Nop.Infrastructure.Json
{
    /// <summary>
    /// 通过辨别器, 把json序列化为真正的子类实例
    /// </summary>
    public class JsonInheritanceConverter : JsonConverter
    {
        private readonly string _discriminator;

        public JsonInheritanceConverter(string discriminator)
        {
            _discriminator = discriminator;
        }

        [ThreadStatic]
        private static bool IsReading;

        [ThreadStatic]
        private static bool IsWriting;

        public override bool CanWrite
        {
            get
            {
                if (!IsWriting)
                {
                    return true;
                }

                return IsWriting = false;
            }
        }

        public override bool CanRead
        {
            get
            {
                if (!IsReading)
                {
                    return true;
                }

                return IsReading = false;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IsReading = true;
            try
            {
                var jsonObject = serializer.Deserialize<JObject>(reader);

                if (jsonObject == null)
                    return null;

                var subName = jsonObject[_discriminator]?.Value<string>();

                if (subName == null)
                {
                    return null;
                }

                var subType = GetObjectSubtype(objectType, subName);

                if (subType == null)
                {
                    return null;
                }

                // 反序列化为具体类的实例
                return serializer.Deserialize(jsonObject.CreateReader(), subType);
            }
            finally
            {
                IsReading = false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IsWriting = true;
            try
            {
                var jsonObject = JObject.FromObject(value, serializer);
                jsonObject.AddFirst(new JProperty(_discriminator, value.GetType().Name));
                writer.WriteToken(jsonObject.CreateReader());
            }
            finally
            {
                IsWriting = false;
            }
        }

        private static Type? GetObjectSubtype(Type objectType, string discriminatorValue)
        {
            KnownTypeAttribute? knownTypeAttribute =
                objectType.GetTypeInfo().GetCustomAttributes<KnownTypeAttribute>()
                    .FirstOrDefault(a => IsKnownType(a, discriminatorValue));

            if (knownTypeAttribute == null) 
            {
                List<KnownTypeAttribute> subTypes = objectType.GetCustomAttributes<KnownTypeAttribute>().Where(x => x.Type?.IsAbstract ?? false).ToList();
                if (subTypes.Any())
                {
                    var knownTypesOfAll = subTypes.Where(x => x.Type != null).SelectMany(x => x.Type.GetCustomAttributes<KnownTypeAttribute>()).Distinct().ToList();

                    return knownTypesOfAll.FirstOrDefault(x => IsKnownType(x, discriminatorValue))?.Type;
                }
                else
                    throw new Exception($"类型{objectType.FullName}的特性没有抽象类, 也没找到 {discriminatorValue} ;");
            }
            return knownTypeAttribute?.Type;
        }

        private static bool IsKnownType(KnownTypeAttribute attribute, string discriminator)
        {
            var type = attribute.Type;

            return type != null && type.Name == discriminator;
        }
    }
}
