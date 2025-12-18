using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;

namespace Nop.Infrastructure.Json
{
    public class JsonSchemaTypeConvert : JsonConverter
    {
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public JsonSchemaTypeConvert()
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JsonSchemaType);
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool isNullable = IsNullableType(objectType);
            Type t = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            if (reader.TokenType == JsonToken.Null)
            {
                if (!IsNullableType(objectType))
                {
                    throw new Exception(string.Format("不能转换null value to {0}.", objectType));
                }

                return null;
            }

            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    string boolText = reader.Value.ToString();
                    if (boolText.Equals("Number", StringComparison.OrdinalIgnoreCase))
                    {
                        return JsonSchemaType.Float;
                    }
                    else
                    {
                        return Enum.Parse<JsonSchemaType>(boolText.ToString());
                    }
                }
                if (reader.TokenType == JsonToken.Integer)
                {
                    string boolText = reader.Value.ToString();

                    if (Enum.TryParse<JsonSchemaType>(boolText.ToString(), out JsonSchemaType temp))
                    {
                        return temp;
                    }
                    else
                        throw new Exception(string.Format("Error converting value {0} to type '{1}'", reader.Value, objectType));

                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error converting value {0} to type '{1}'", reader.Value, objectType));
            }
            throw new Exception(string.Format("Unexpected token {0} when parsing enum", reader.TokenType));
        }





        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            JsonSchemaType bValue = (JsonSchemaType)value;

            if (bValue == JsonSchemaType.Float)
            {
                writer.WriteValue("Number");
            }
            else
            {
                writer.WriteValue(bValue.ToString());
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
    }
}
