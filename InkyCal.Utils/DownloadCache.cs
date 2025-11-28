using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InkyCal.Utils.Caching;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// 
	/// </summary>
	public static class DownloadCache
	{

		private static readonly HttpClient client = new HttpClient();

		private static IImageCacheService _cache;

		/// <summary>
		/// Sets the cache service to use. Must be called before using LoadCachedContent.
		/// </summary>
		/// <param name="cacheService">The cache service to use.</param>
		public static void SetCacheService(IImageCacheService cacheService)
		{
			_cache = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
		}

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
			// Fallback to in-memory cache if not initialized
			_cache ??= new MemoryCacheService();

			using (MiniProfiler.Current.Step($"Loading url results from cache"))
			{
				return await _cache.GetOrCreateAsync(imageUrl, async () =>
				{
					// Key not in cache, so get data.
					using (MiniProfiler.Current.Step($"Response content not in cache, loading from URL"))
					{
						var result = await client.GetAsync(imageUrl.ToString(), cancellationToken);
						result.EnsureSuccessStatusCode();
						return await result.Content.ReadAsByteArrayAsync(cancellationToken);
					}
				}, expiration
				);
			}
		}
	}
}
