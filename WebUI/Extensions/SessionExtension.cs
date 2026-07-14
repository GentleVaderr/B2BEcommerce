using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WebUI.Extensions
{
    public static class SessionExtension
    {
        // Session'a nesne kaydetme
        public static void SetJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Session'dan nesne okuma
        public static T? GetJson<T>(this ISession session, string key)
        {
            var sessionData = session.GetString(key);
            return sessionData == null ? default : JsonSerializer.Deserialize<T>(sessionData);
        }
    }
}