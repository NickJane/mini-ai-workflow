using Newtonsoft.Json;

namespace Nop.Infrastructure.Json
{
    public class DateTimeJsonConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }


        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (value is DateTime dt)
            {
                writer.WriteValue(dt.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            }
        }

        public bool IsNullableType(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t");
            }
            if (t.IsGenericType == false)
            {
                return (t.BaseType.FullName == "System.ValueType");
            }
            return (t.BaseType.FullName == "System.ValueType" && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}