using System;
using System.Runtime.Serialization;
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
		public Task<(bool Found, byte[] Value)> TryGetValueAsync<T>(T key) where T : ISerializable, IEquatable<T>
		{
			var found = _cache.TryGetValue(key, out byte[] value);
			return Task.FromResult((found, value));
		}

		/// <inheritdoc/>
		public Task<(bool Found, byte[] Value)> TryGetValueAsync(string key)
		{
			var found = _cache.TryGetValue(key, out byte[] value);
			return Task.FromResult((found, value));
		}

		/// <inheritdoc/>
		public Task SetAsync<T>(T key, byte[] value, TimeSpan expiration) where T : ISerializable, IEquatable<T>
		{
			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetAbsoluteExpiration(expiration);

				cacheEntryOptions.SetSize(value.Length);

			_cache.Set(key, value, cacheEntryOptions);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public Task SetAsync(string key, byte[] value, TimeSpan expiration)
		{
			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetAbsoluteExpiration(expiration);

			cacheEntryOptions.SetSize(value.Length);

			_cache.Set(key, value, cacheEntryOptions);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public async Task<byte[]> GetOrCreateAsync<T>(T key, Func<Task<byte[]>> factory, TimeSpan expiration) where T : ISerializable, IEquatable<T>
		{
			ArgumentNullException.ThrowIfNull(factory);

			return await _cache.GetOrCreateAsync(key, async entry =>
			{
				entry.SetAbsoluteExpiration(expiration);
				
				var value = await factory();

				entry.SetSize(value.LongLength);

				return value;
			});
		}

		/// <inheritdoc/>
		public int Count() => _cache.Count;
	}
}
