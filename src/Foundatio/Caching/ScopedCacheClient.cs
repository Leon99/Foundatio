﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundatio.Utility;

namespace Foundatio.Caching {
    public class ScopedHybridCacheClient : ScopedCacheClient, IHybridCacheClient {
        public ScopedHybridCacheClient(IHybridCacheClient client, string scope = null) : base(client, scope) {}
    }

    public class ScopedCacheClient : ICacheClient {
        private string _keyPrefix;
        private bool _isLocked;
        private readonly object _lock = new object();

        public ScopedCacheClient(ICacheClient client, string scope = null) {
            UnscopedCache = client ?? new NullCacheClient();
            _isLocked = scope != null;
            Scope = !String.IsNullOrWhiteSpace(scope) ? scope.Trim() : null;

            _keyPrefix = Scope != null ? String.Concat(Scope, ":") : String.Empty;
        }

        public ICacheClient UnscopedCache { get; private set; }

        public string Scope { get; private set; }

        public void SetScope(string scope) {
            if (_isLocked)
                throw new InvalidOperationException("Scope can't be changed after it has been set.");

            lock (_lock) {
                if (_isLocked)
                    throw new InvalidOperationException("Scope can't be changed after it has been set.");

                _isLocked = true;
                Scope = !String.IsNullOrWhiteSpace(scope) ? scope.Trim() : null;
                _keyPrefix = Scope != null ? String.Concat(Scope, ":") : String.Empty;
            }
        }

        protected string GetScopedCacheKey(string key) {
            return String.Concat(_keyPrefix, key);
        }

        protected IEnumerable<string> GetScopedCacheKeys(IEnumerable<string> keys) {
            return keys?.Select(GetScopedCacheKey);
        }

        protected string UnscopeCacheKey(string scopedKey) {
            return scopedKey?.Substring(_keyPrefix.Length);
        }

        public Task<bool> RemoveAsync(string key) {
            return UnscopedCache.RemoveAsync(GetScopedCacheKey(key));
        }

        public Task<bool> RemoveIfEqualAsync<T>(string key, T expected) {
            return UnscopedCache.RemoveIfEqualAsync(GetScopedCacheKey(key), expected);
        }

        public Task<int> RemoveAllAsync(IEnumerable<string> keys = null) {
            if (keys == null)
                return RemoveByPrefixAsync(String.Empty);

            return UnscopedCache.RemoveAllAsync(GetScopedCacheKeys(keys));
        }

        public Task<int> RemoveByPrefixAsync(string prefix) {
            return UnscopedCache.RemoveByPrefixAsync(GetScopedCacheKey(prefix));
        }

        public Task<CacheValue<T>> GetAsync<T>(string key) {
            return UnscopedCache.GetAsync<T>(GetScopedCacheKey(key));
        }

        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> keys) {
            var scopedDictionary = await UnscopedCache.GetAllAsync<T>(GetScopedCacheKeys(keys)).AnyContext();
            return scopedDictionary.ToDictionary(kvp => UnscopeCacheKey(kvp.Key), kvp => kvp.Value);
        }

        public Task<bool> AddAsync<T>(string key, T value, TimeSpan? expiresIn = null) {
            return UnscopedCache.AddAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<int> SetAllAsync<T>(IDictionary<string, T> values, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetAllAsync(values?.ToDictionary(kvp => GetScopedCacheKey(kvp.Key), kvp => kvp.Value), expiresIn);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan? expiresIn = null) {
            return UnscopedCache.ReplaceAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<bool> ReplaceIfEqualAsync<T>(string key, T value, T expected, TimeSpan? expiresIn = null) {
            return UnscopedCache.ReplaceIfEqualAsync(GetScopedCacheKey(key), value, expected, expiresIn);
        }

        public Task<double> IncrementAsync(string key, double amount, TimeSpan? expiresIn = null) {
            return UnscopedCache.IncrementAsync(GetScopedCacheKey(key), amount, expiresIn);
        }

        public Task<long> IncrementAsync(string key, long amount, TimeSpan? expiresIn = null) {
            return UnscopedCache.IncrementAsync(GetScopedCacheKey(key), amount, expiresIn);
        }
        
        public Task<bool> ExistsAsync(string key) {
            return UnscopedCache.ExistsAsync(GetScopedCacheKey(key));
        }

        public Task<TimeSpan?> GetExpirationAsync(string key) {
            return UnscopedCache.GetExpirationAsync(GetScopedCacheKey(key));
        }

        public Task SetExpirationAsync(string key, TimeSpan expiresIn) {
            return UnscopedCache.SetExpirationAsync(GetScopedCacheKey(key), expiresIn);
        }

        public Task<double> SetIfHigherAsync(string key, double value, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetIfHigherAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<long> SetIfHigherAsync(string key, long value, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetIfHigherAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<double> SetIfLowerAsync(string key, double value, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetIfLowerAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<long> SetIfLowerAsync(string key, long value, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetIfLowerAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<long> ListAddAsync<T>(string key, IEnumerable<T> values, TimeSpan? expiresIn = null) {
            return UnscopedCache.ListAddAsync(GetScopedCacheKey(key), values, expiresIn);
        }

        public Task<long> ListRemoveAsync<T>(string key, IEnumerable<T> values, TimeSpan? expiresIn = null) {
            return UnscopedCache.ListRemoveAsync(GetScopedCacheKey(key), values, expiresIn);
        }

        public Task<CacheValue<ICollection<T>>> GetListAsync<T>(string key, int? page = null, int pageSize = 100) {
            return UnscopedCache.GetListAsync<T>(GetScopedCacheKey(key), page, pageSize);
        }

        public void Dispose() {}
    }
}
