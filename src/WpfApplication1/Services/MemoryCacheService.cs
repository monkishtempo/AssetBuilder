using System;
using Microsoft.Extensions.Caching.Memory;
using CacheItemPriority = Microsoft.Extensions.Caching.Memory.CacheItemPriority;
using MemoryCache = Microsoft.Extensions.Caching.Memory.MemoryCache;

namespace AssetBuilder.Services
{
    public class MemoryCacheService<T> : ICacheService<T>
    {
        private bool _disposedValue;

        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions{SizeLimit = 2048});

        public T GetOrCreate(object key, Func<T> createItem)
        {
            if (!_cache.TryGetValue(key, out T cacheEntry))
            {
                cacheEntry = createItem();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                
                _cache.Set(key, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cache.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}