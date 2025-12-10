using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace InkyCal.Utils.Caching
{
	/// <summary>
	/// In-memory cache implementation using Microsoft.Extensions.Caching.Memory
	/// </summary>
	public class MemoryCacheService : IImageCacheService
	{
		private readonly MemoryCache _cache;
		private long _approximateSize;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryCacheService"/> class.
		/// </summary>
		/// <param name="sizeLimit">The maximum size of the cache in bytes (default: 500 MB).</param>
		public MemoryCacheService(long sizeLimit = 1024 * 1024 * 500)
		{
			_cache = new MemoryCache(new MemoryCacheOptions()
			{
				SizeLimit = sizeLimit,
			});
		}

		/// <inheritdoc/>
		public Task<(bool Found, byte[] Value)> TryGetValueAsync(string key)
		{
			var found = _cache.TryGetValue(key, out byte[] value);
			return Task.FromResult((found, value));
		}
		
		/// <inheritdoc/>
		public Task<(bool Found, byte[] Value)> TryGetValueAsync<T>(T key) where T : IEquatable<T>
		{
			var found = _cache.TryGetValue(key, out byte[] value);
			return Task.FromResult((found, value));
		}

		/// <inheritdoc/>
		public Task SetAsync<T>(T key, byte[] value, TimeSpan expiration) where T : IEquatable<T>
		{
			var actualSize = value?.Length ?? 0;

			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetAbsoluteExpiration(expiration)
				.RegisterPostEvictionCallback((k, v, reason, state) =>
				{
					// Decrease size when item is evicted
					if (v is byte[] bytes)
						Interlocked.Add(ref _approximateSize, -bytes.Length);
				});

			if (actualSize > 0)
				cacheEntryOptions.SetSize(actualSize);

			_cache.Set(key, value, cacheEntryOptions);

			// Track approximate size
			Interlocked.Add(ref _approximateSize, actualSize);

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public Task SetAsync(string key, byte[] value, TimeSpan expiration)
		{
			var actualSize = value?.Length ?? 0;
			
			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetAbsoluteExpiration(expiration)
				.RegisterPostEvictionCallback((k, v, reason, state) =>
				{
					// Decrease size when item is evicted
					if (v is byte[] bytes)
						Interlocked.Add(ref _approximateSize, -bytes.Length);
				});

			if (actualSize > 0)
				cacheEntryOptions.SetSize(actualSize);

			_cache.Set(key, value, cacheEntryOptions);
			
			// Track approximate size
			Interlocked.Add(ref _approximateSize, actualSize);

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public async Task<byte[]> GetOrCreateAsync(string key, Func<Task<byte[]>> factory, TimeSpan expiration)
		{
			ArgumentNullException.ThrowIfNull(factory);

			var (found, value) = await TryGetValueAsync(key);
			if (found)
				return value;

			// Create the value
			value = await factory();

			// Cache it
			await SetAsync(key, value, expiration);

			return value;
		}

		/// <inheritdoc/>
		public async Task<byte[]> GetOrCreateAsync<T>(T key, Func<Task<byte[]>> factory, TimeSpan expiration) where T : IEquatable<T>
		{
			ArgumentNullException.ThrowIfNull(factory);

			var (found, value) = await TryGetValueAsync(key);
			if (found)
				return value;

			// Create the value
			value = await factory();

			// Cache it
			await SetAsync(key, value, expiration);

			return value;
		}

		/// <inheritdoc/>
		public int Count() => _cache.Count;

		/// <inheritdoc/>
		public long GetApproximateSize() => Interlocked.Read(ref _approximateSize);
	}
}
