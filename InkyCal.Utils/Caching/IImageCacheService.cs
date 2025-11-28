using System;
using System.Threading.Tasks;

namespace InkyCal.Utils.Caching
{
	/// <summary>
	/// Abstraction for caching byte array data (images, PDFs, etc.)
	/// </summary>
	public interface IImageCacheService
	{
		/// <summary>
		/// Tries to get a cached value.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>A tuple containing whether the value was found and the cached value if found.</returns>
		Task<(bool Found, byte[] Value)> TryGetValueAsync(string key);

		/// <summary>
		/// Sets a value in the cache.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The value to cache.</param>
		/// <param name="expiration">The expiration time.</param>
		/// <param name="size">Optional size of the cached item for memory-based caches.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SetAsync(string key, byte[] value, TimeSpan expiration, long? size = null);

		/// <summary>
		/// Gets the number of cached entries (if supported by the implementation).
		/// </summary>
		/// <returns>The number of cached entries, or -1 if not supported.</returns>
		int Count();
	}
}
