using System;
using System.Runtime.Serialization;
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
		Task<(bool Found, byte[] Value)> TryGetValueAsync<T>(T key) where T : ISerializable, IEquatable<T>;

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
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SetAsync<T>(T key, byte[] value, TimeSpan expiration) where T : ISerializable, IEquatable<T>;

		/// <summary>
		/// Sets a value in the cache.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The value to cache.</param>
		/// <param name="expiration">The expiration time.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SetAsync(string key, byte[] value, TimeSpan expiration);

		/// <summary>
		/// Gets a cached value or creates it if it doesn't exist.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="factory">A factory function to create the value if not found in cache.</param>
		/// <param name="expiration">The expiration time for the cached value.</param>
		/// <returns>The cached or newly created value.</returns>
		Task<byte[]> GetOrCreateAsync<T>(T key, Func<Task<byte[]>> factory, TimeSpan expiration) where T: ISerializable,IEquatable<T>;

		/// <summary>
		/// Gets the number of cached entries (if supported by the implementation).
		/// </summary>
		/// <returns>The number of cached entries, or -1 if not supported.</returns>
		int Count();

		/// <summary>
		/// Gets the approximate size of the cache in bytes (if supported by the implementation).
		/// </summary>
		/// <returns>The approximate cache size in bytes, or -1 if not supported.</returns>
		long GetApproximateSize();
	}
}
