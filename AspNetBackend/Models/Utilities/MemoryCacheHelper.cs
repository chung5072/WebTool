using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace AspNetBackend.Models.Utilities
{
    public class MemoryCacheHelper
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;

        public static T GetFromCache<T>(string key) where T : class
        {
            if (Cache.Contains(key))
            {
                var cachedValue = Cache[key] as string;
                return cachedValue != null ? JsonConvert.DeserializeObject<T>(cachedValue) : null;
            }
            return null;
        }

        public static void AddToCache(string key, object value, int durationInMinutes = 10)
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            Cache.Set(key, serializedValue, DateTimeOffset.Now.AddMinutes(durationInMinutes));
        }
    }
}