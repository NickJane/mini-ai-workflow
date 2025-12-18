using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Nop.Infrastructure
{
    public static class JTokenExtension
    {
        public static bool IsNull(this JToken token)
        {
            if (token == null)
            {
                return true;
            }

            if (token.Type == JTokenType.Null)
            {
                return true;
            }

            if (token is JValue value)
            {
                return value.Value == null;
            }

            return false;
        }

        public static JArray Sort(this JArray ja)
        {
            if (ja == null) return null;
            try
            {
                List<string> lst = new List<string>();
                foreach (var item in ja.Children())
                {
                    lst.Add(item.ToString());
                }
                lst.Sort();

                JArray newArr = new JArray();
                lst.ForEach(x =>
                {
                    newArr.Add(JToken.Parse(x));
                });

                return newArr;
            }
            catch
            {
                return ja;
            }
        }
    }
}
