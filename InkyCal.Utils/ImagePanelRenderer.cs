using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.Primitives;
using StackExchange.Profiling;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace InkyCal.Utils
{

	/// <summary>
	/// An image panel, assumes a landscape image, resizes and flips it to portait.
	/// </summary>
	public class ImagePanelRenderer : IPanelRenderer
	{
		private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions()
		{
			SizeLimit = 1024*1024,
		});

		/// <summary>
		/// Returns a cached image
		/// </summary>
		/// <returns></returns>
		private static async Task<byte[]> LoadCachedImage(Uri imageUrl)
		{

			using (MiniProfiler.Current.Step($"Loading image from cache"))
			{
				if (!_cache.TryGetValue(imageUrl.ToString(), out byte[] cacheEntry))// Look for cache key.
				{
					// Key not in cache, so get data.
					using (MiniProfiler.Current.Step($"Image not in cache, loading from URL"))
						cacheEntry = await client.GetByteArrayAsync(imageUrl.ToString());

					var cacheEntryOptions = new MemoryCacheEntryOptions()
						.SetSize(cacheEntry.Length)
						// Remove from cache after this time, regardless of sliding expiration
						.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

					// Save data in cache.
					using (MiniProfiler.Current.Step($"Storing image in cache"))
						_cache.Set(imageUrl.ToString(), cacheEntry, cacheEntryOptions);
				}

				return cacheEntry;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="imageUrl"></param>
		/// <param name="rotateImage"></param>
		public ImagePanelRenderer(Uri imageUrl, RotateMode rotateImage = RotateMode.None)
		{
			this.imageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
			this.rotateImage = rotateImage;
			//cachedImage = new Lazy<byte[]>(GetTestImage);
		}

		//private readonly Lazy<byte[]> cachedImage;

		private static readonly HttpClient client = new HttpClient();
		private readonly Uri imageUrl;
		private readonly RotateMode rotateImage;

		//private byte[] GetTestImage()
		//{
		//	return client.GetByteArrayAsync(imageUrl.ToString()).Result;
		//}

		/// <inheritdoc/>
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
		{
			return await Task.Run(async () =>
			{
				if (colors is null)
					colors = new[] { Color.White, Color.Black };

				Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image;
				try
				{
					image = Image.Load(await LoadCachedImage(imageUrl));
				}
				catch (UnknownImageFormatException ex) {
					ex.Data["ImageUrl"] = imageUrl;
					throw;
				}

				using (MiniProfiler.Current.Step($"Resizing image and reducing image palette"))
					image.Mutate(x => x
					.Rotate(rotateImage)
					.Resize(new ResizeOptions() { Mode = ResizeMode.Crop, Size = new Size(width, height) })
					.BackgroundColor(Color.White)
					.Quantize(new PaletteQuantizer(colors))
					);

				return image;
			});
		}
	}
}
