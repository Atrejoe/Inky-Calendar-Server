using System;
using System.Threading.Tasks;

namespace InkyCal.Utils.Caching
{
	/// <summary>
	/// Abstraction for caching string data (calendar content, etc.)
	/// </summary>
	public interface IStringCacheService
	{
		/// <summary>
		/// Tries to get a cached value.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>The cached value if found, null otherwise.</returns>
		Task<(bool Found, string Value)> TryGetValueAsync(string key);

		/// <summary>
		/// Sets a value in the cache.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The value to cache.</param>
		/// <param name="expiration">The expiration time.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SetAsync(string key, string value, TimeSpan expiration);

		/// <summary>
		/// Gets the number of cached entries (if supported by the implementation).
		/// </summary>
		/// <returns>The number of cached entries, or -1 if not supported.</returns>
		int Count();
	}
}
