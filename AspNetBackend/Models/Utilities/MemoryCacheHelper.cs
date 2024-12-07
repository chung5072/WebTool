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

        public static object GetFromCache(string key)
        {
            return Cache.Contains(key) ? Cache[key] : null;
        }

        public static void AddToCache(string key, object value, int durationInMinutes = 10)
        {
            Cache.Set(key, value, DateTimeOffset.Now.AddMinutes(durationInMinutes));
        }
    }
}