using System;
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
		public Task<(bool Found, byte[] Value)> TryGetValueAsync(string key)
		{
			var found = _cache.TryGetValue(key, out byte[] value);
			return Task.FromResult((found, value));
		}

		/// <inheritdoc/>
		public Task SetAsync(string key, byte[] value, TimeSpan expiration, long? size = null)
		{
			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetAbsoluteExpiration(expiration);

			if (size.HasValue)
				cacheEntryOptions.SetSize(size.Value);

			_cache.Set(key, value, cacheEntryOptions);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public int Count() => _cache.Count;
	}
}
