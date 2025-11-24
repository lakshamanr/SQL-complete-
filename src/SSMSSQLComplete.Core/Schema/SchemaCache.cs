using System;
using System.Collections.Concurrent;

namespace SSMSSQLComplete.Core.Schema
{
    public class SchemaCache
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache;
        private readonly TimeSpan _defaultExpiration;
        private readonly int _maxCacheSize;
        private readonly object _evictionLock = new object();

        public SchemaCache(TimeSpan? expiration = null, int maxSize = 10)
        {
            _cache = new ConcurrentDictionary<string, CacheEntry>();
            _defaultExpiration = expiration ?? TimeSpan.FromMinutes(30);
            _maxCacheSize = maxSize;
        }

        public void Set(string key, DatabaseMetadata metadata)
        {
            // Enforce cache size limit with atomic check-and-evict
            lock (_evictionLock)
            {
                if (_cache.Count >= _maxCacheSize)
                {
                    EvictOldest();
                }

                var entry = new CacheEntry
                {
                    Metadata = metadata,
                    Timestamp = DateTime.UtcNow,
                    Expiration = _defaultExpiration
                };

                _cache.AddOrUpdate(key, entry, (k, old) => entry);
            }

            Infrastructure.Logger.Instance.Info($"Schema cached for key: {key}");
        }

        public bool TryGet(string key, out DatabaseMetadata metadata)
        {
            metadata = null;

            if (_cache.TryGetValue(key, out var entry))
            {
                if (!entry.IsExpired)
                {
                    metadata = entry.Metadata;
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

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cache.Clear();
            Infrastructure.Logger.Instance.Info("Schema cache cleared");
        }

        private void EvictOldest()
        {
            DateTime oldestTime = DateTime.MaxValue;
            string oldestKey = null;

            foreach (var kvp in _cache)
            {
                if (kvp.Value.Timestamp < oldestTime)
                {
                    oldestTime = kvp.Value.Timestamp;
                    oldestKey = kvp.Key;
                }
            }

            if (oldestKey != null)
            {
                _cache.TryRemove(oldestKey, out _);
            }
        }

        private class CacheEntry
        {
            public DatabaseMetadata Metadata { get; set; }
            public DateTime Timestamp { get; set; }
            public TimeSpan Expiration { get; set; }

            public bool IsExpired => DateTime.UtcNow - Timestamp > Expiration;
        }
    }
}
