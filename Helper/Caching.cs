using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Helper
{
    public class Caching : ICaching
    {
        private IMemoryCache cache;
        public Caching(IMemoryCache cache)
        {
            this.cache = cache;
        }
        public object Get(string cacheKey)
        {
            return cache.Get(cacheKey);
        }

        public void Set(string cacheKey, object cacheValue)
        {
            cache.Set(cacheKey, cacheValue, TimeSpan.FromSeconds(7000));
        }
    }
}
