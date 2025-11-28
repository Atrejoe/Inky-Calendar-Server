using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace InkyCal.Utils.Caching
{
	/// <summary>
	/// In-memory string cache implementation
	/// </summary>
	public class MemoryStringCacheService : IStringCacheService
	{
		private readonly MemoryCache _cache;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryStringCacheService"/> class.
		/// </summary>
		/// <param name="sizeLimit">The maximum number of entries in the cache.</param>
		public MemoryStringCacheService(long sizeLimit = 1024)
		{
			_cache = new MemoryCache(new MemoryCacheOptions()
			{
				SizeLimit = sizeLimit,
			});
		}

		/// <inheritdoc/>
		public Task<(bool Found, string Value)> TryGetValueAsync(string key)
		{
			var found = _cache.TryGetValue(key, out string value);
			return Task.FromResult((found, value));
		}

		/// <inheritdoc/>
		public Task SetAsync(string key, string value, TimeSpan expiration)
		{
			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetSize(1)
				.SetAbsoluteExpiration(expiration);

			_cache.Set(key, value, cacheEntryOptions);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public int Count() => _cache.Count;
	}
}
