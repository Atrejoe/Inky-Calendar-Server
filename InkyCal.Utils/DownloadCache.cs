using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// 
	/// </summary>
	public static class DownloadCache
	{

		private static readonly HttpClient client = new HttpClient();

		private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions()
		{
			SizeLimit = 1024 * 1024 * 500,
		});


		/// <summary>
		/// Returns a (10-minute) cached image
		/// </summary>
		/// <returns></returns>
		internal static async Task<byte[]> LoadCachedContent(this Uri imageUrl, CancellationToken cancellationToken = default) 
			=> await LoadCachedContent(imageUrl, TimeSpan.FromMinutes(10), cancellationToken);

		/// <summary>
		/// Returns a cached image
		/// </summary>
		/// <returns></returns>
		/// <exception cref="HttpRequestException">When download failed (non-200 response was returned))</exception>
		internal static async Task<byte[]> LoadCachedContent(this Uri imageUrl, TimeSpan expiration, CancellationToken cancellationToken = default)
		{

			using (MiniProfiler.Current.Step($"Loading url results from cache"))
			{
				if (!_cache.TryGetValue(imageUrl.ToString(), out byte[] cacheEntry))// Look for cache key.
				{
					// Key not in cache, so get data.
					using (MiniProfiler.Current.Step($"Response content not in cache, loading from URL"))
					{
						var result = await client.GetAsync(imageUrl.ToString(), cancellationToken);
						result.EnsureSuccessStatusCode();
						cacheEntry = await result.Content.ReadAsByteArrayAsync(cancellationToken);
					}

					var cacheEntryOptions = new MemoryCacheEntryOptions()
						.SetSize(cacheEntry.Length)
						// Remove from cache after this time, regardless of sliding expiration
						.SetAbsoluteExpiration(expiration);

					// Save data in cache.
					using (MiniProfiler.Current.Step($"Storing response content ({cacheEntry.Length:n0} bytes) in cache"))
						_cache.Set(imageUrl.ToString(), cacheEntry, cacheEntryOptions);
				}

				return cacheEntry;
			}
		}
	}
}
