using InkyCal.Models;
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
	/// 
	/// </summary>
	/// <seealso cref="InkyCal.Models.PanelCacheKey" />
	public class ImagePanelCacheKey : PanelCacheKey
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ImagePanelCacheKey"/> class.
		/// </summary>
		/// <param name="expiration">The expiration.</param>
		/// <param name="imageUrl">The image URL.</param>
		/// <param name="rotateImage">The rotate image.</param>
		public ImagePanelCacheKey(TimeSpan expiration, Uri imageUrl, RotateMode rotateImage) : base(expiration)
		{
			ImageUrl = imageUrl;
			RotateImage = rotateImage;
		}

		/// <summary>
		/// Gets the image URL.
		/// </summary>
		/// <value>
		/// The image URL.
		/// </value>
		public Uri ImageUrl { get; }
		/// <summary>
		/// Gets the rotate image.
		/// </summary>
		/// <value>
		/// The rotate image.
		/// </value>
		public RotateMode RotateImage { get; }

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ImageUrl, RotateImage);

		/// <summary>
		/// Indicates whether the current object is equal to another <see cref="T:InkyCal.Models.PanelCacheKey" /> (or derived class))
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		protected override bool Equals(PanelCacheKey other)
		{
			return other is ImagePanelCacheKey ipc
				&& ipc.ImageUrl.Equals(ImageUrl)
				&& ipc.RotateImage.Equals(RotateImage);
		}
	}

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

			cacheKey = new ImagePanelCacheKey(expiration: TimeSpan.FromMinutes(1), imageUrl, rotateImage);
		}

		private static readonly HttpClient client = new HttpClient();
		private readonly Uri imageUrl;
		private readonly RotateMode rotateImage;
		private readonly ImagePanelCacheKey cacheKey;

		PanelCacheKey IPanelRenderer.CacheKey => cacheKey;

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
					.BackgroundColor(Color.Transparent)
					.Quantize(new PaletteQuantizer(colors))
					);

				return image;
			});
		}

	}
}
