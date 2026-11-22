using InkyCal.Models;
using InkyCal.Utils.Caching;

namespace InkyCal.Utils
{
	/// <summary>
	/// Helper class for initializing PDF renderer cache service
	/// </summary>
	public static class PdfRendererHelper
	{
		/// <summary>
		/// Sets the cache service for all PdfRenderer instances
		/// </summary>
		/// <param name="cacheService">The cache service to use</param>
		public static void SetCacheService(IImageCacheService cacheService)
		{
			// Since PdfRenderer<T> is generic, we need to call SetCacheService
			// through a concrete type. The static field is shared across all generic instances.
			// We'll use Panel as a dummy type parameter.
			PdfRenderer<Panel>.SetCacheService(cacheService);
		}
	}
}
