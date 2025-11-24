using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SSMSSQLComplete.Core.Infrastructure
{
    /// <summary>
    /// Thread-safe cache with expiration support
    /// </summary>
    public class ConcurrentCache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, CacheEntry> _cache;
        private readonly TimeSpan _defaultExpiration;
        private readonly Timer _cleanupTimer;

        public ConcurrentCache(TimeSpan? expiration = null)
        {
            _cache = new ConcurrentDictionary<TKey, CacheEntry>();
            _defaultExpiration = expiration ?? TimeSpan.FromMinutes(10);

            // Setup cleanup timer to run every minute
            _cleanupTimer = new Timer(
                _ => CleanupExpiredEntries(),
                null,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(1));
        }

        public void Set(TKey key, TValue value, TimeSpan? expiration = null)
        {
            var entry = new CacheEntry
            {
                Value = value,
                ExpirationTime = DateTime.UtcNow + (expiration ?? _defaultExpiration)
            };

            _cache.AddOrUpdate(key, entry, (k, old) => entry);
        }

        public bool TryGet(TKey key, out TValue value)
        {
            value = default;

            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpirationTime > DateTime.UtcNow)
                {
                    value = entry.Value;
                    return true;
                }
                else
                {
                    // Remove expired entry
                    _cache.TryRemove(key, out _);
                }
            }

            return false;
        }

        public void Remove(TKey key)
        {
            _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private void CleanupExpiredEntries()
        {
            var now = DateTime.UtcNow;

            foreach (var kvp in _cache)
            {
                if (kvp.Value.ExpirationTime <= now)
                {
                    _cache.TryRemove(kvp.Key, out _);
                }
            }
        }

        private class CacheEntry
        {
            public TValue Value { get; set; }
            public DateTime ExpirationTime { get; set; }
        }
    }
}
