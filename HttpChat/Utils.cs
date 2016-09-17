using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HttpChat
{
    public static class U
    {
        ///Session extension to store object in session easily
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
        
    }
}