using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Schema
{
    public sealed class SchemaService
    {
        private static readonly Lazy<SchemaService> _instance =
            new Lazy<SchemaService>(() => new SchemaService());

        public static SchemaService Instance => _instance.Value;

        private readonly SchemaCache _cache;
        private readonly ISchemaProvider _provider;
        private readonly SemaphoreSlim _refreshLock;
        private string _currentConnectionString;
        private string _currentDatabase;

        private SchemaService()
        {
            _cache = new SchemaCache();
            _provider = new SchemaFetcher();
            _refreshLock = new SemaphoreSlim(1, 1);
        }

        public bool IsSchemaLoaded { get; private set; }

        public void SetConnection(string connectionString, string databaseName)
        {
            _currentConnectionString = connectionString;
            _currentDatabase = databaseName;

            Infrastructure.Logger.Instance.Info($"Connection set to database: {databaseName}");
        }

        public async Task<DatabaseMetadata> GetCurrentDatabaseMetadataAsync()
        {
            if (string.IsNullOrEmpty(_currentConnectionString) ||
                string.IsNullOrEmpty(_currentDatabase))
            {
                return null;
            }

            var cacheKey = GetCacheKey(_currentConnectionString, _currentDatabase);

            // Try to get from cache first
            if (_cache.TryGet(cacheKey, out var metadata))
            {
                return metadata;
            }

            // Fetch from database
            return await RefreshSchemaAsync();
        }

        public async Task<DatabaseMetadata> RefreshSchemaAsync()
        {
            if (string.IsNullOrEmpty(_currentConnectionString) ||
                string.IsNullOrEmpty(_currentDatabase))
            {
                return null;
            }

            await _refreshLock.WaitAsync();
            try
            {
                var cacheKey = GetCacheKey(_currentConnectionString, _currentDatabase);

                Infrastructure.Logger.Instance.Info($"Refreshing schema for: {_currentDatabase}");

                var metadata = await _provider.FetchMetadataAsync(_currentConnectionString, _currentDatabase);

                _cache.Set(cacheKey, metadata);
                IsSchemaLoaded = true;

                Infrastructure.TelemetryService.Instance.TrackEvent("SchemaRefreshed", new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Database", _currentDatabase },
                    { "TableCount", metadata.Tables.Count.ToString() },
                    { "ViewCount", metadata.Views.Count.ToString() }
                });

                return metadata;
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error refreshing schema: {ex.Message}", ex);
                IsSchemaLoaded = false;
                return null;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public void ClearCache()
        {
            _cache.Clear();
            IsSchemaLoaded = false;
        }

        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            return await _provider.TestConnectionAsync(connectionString);
        }

        private string GetCacheKey(string connectionString, string database)
        {
            // Create a simple hash-based key
            var hash = $"{connectionString}|{database}".GetHashCode();
            return $"schema_{database}_{hash}";
        }
    }
}
