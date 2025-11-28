using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ImageMagick;
using InkyCal.Models;
using InkyCal.Utils.Caching;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// Image settings for cache key generation
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see cref="ImageSettings"/> class.
	/// </remarks>
	/// <param name="width">The width.</param>
	/// <param name="height">The height.</param>
	/// <param name="colors">The colors.</param>
	/// <exception cref="ArgumentNullException">colors</exception>
	[Serializable]
	public sealed class ImageSettings(int width, int height, Color[] colors) : IEquatable<ImageSettings>, ISerializable
	{
		/// <summary>
		/// Gets the width of an image
		/// </summary>
		/// <value>
		/// The width.
		/// </value>
		public int Width { get; } = width;

		/// <summary>
		/// Gets the height of an image
		/// </summary>
		/// <value>
		/// The height.
		/// </value>
		public int Height { get; } = height;

		/// <summary>
		/// Gets the color palette of an image
		/// </summary>
		/// <value>
		/// The colors.
		/// </value>
		public Color[] Colors { get; } = colors ?? throw new ArgumentNullException(nameof(colors));

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization info</param>
		/// <param name="context">The streaming context</param>
		private ImageSettings(SerializationInfo info, StreamingContext context)
			: this(
				info.GetInt32(nameof(Width)),
				info.GetInt32(nameof(Height)),
				DeserializeColors(info))
		{
		}

		private static Color[] DeserializeColors(SerializationInfo info)
		{
			var count = info.GetInt32("ColorsCount");
			var colors = new Color[count];
			for (int i = 0; i < count; i++)
			{
				var pixel = Rgba32.ParseHex(info.GetString($"Color_{i}"));

				colors[i] = Color.FromPixel<SixLabors.ImageSharp.PixelFormats.Rgba32>(pixel);
			}
			return colors;
		}

		/// <inheritdoc/>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			ArgumentNullException.ThrowIfNull(info);

			info.AddValue(nameof(Width), Width);
			info.AddValue(nameof(Height), Height);
			info.AddValue("ColorsCount", Colors.Length);
			for (int i = 0; i < Colors.Length; i++)
			{
				var rgba32 = Colors[i].ToPixel<SixLabors.ImageSharp.PixelFormats.Rgba32>();
				info.AddValue($"Color_{i}", rgba32.ToHex());
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is ImageSettings other
				&& Equals(other);

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		public bool Equals(ImageSettings other) => other != null
				&& Width.Equals(other.Width)
				&& Height.Equals(other.Height)
				&& Colors.SequenceEqual(other.Colors)
				;


		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(
												Width.GetHashCode(),
												Height.GetHashCode(),
												// Array itself cannot be used in HashCoodde.Combin,nor can it return a sensible hashcode
												// Use reproducible attributes
												Colors.Length,
												Colors.Average(x => x.GetHashCode())
											);
	}

	/// <summary>
	/// Signature for a helper class for rendering a panel.
	/// </summary>
	public interface IPanelRenderer
	{
		/// <summary>
		/// Returns an image in portrait mode, while width and height are in landscape mode
		/// </summary>
		/// <param name="width">The height of the panel (in landscape mode).</param>
		/// <param name="height">The width of the panel (in landscape mode).</param>
		/// <param name="colors">The number of colors to render in.</param>
		/// <param name="log"></param>
		/// <returns>An image</returns>
		/// <remarks>Maybe it wise to keep evrything in portrait mode</remarks>
		Task<Image> GetImage(int width, int height, Color[] colors, Log log);

		/// <summary>
		/// A callback method for logging exceptions to
		/// </summary>
		/// <param name="ex">The exception to log</param>
		/// <param name="handled">if set to <c>true</c> if the exception it deemed to be handled (and will not be reported as exception).</param>
		/// <param name="explanation">Explanation o the exception, or why it was deemed to be handled, optional.</param>
		public delegate Task Log(Exception ex, bool handled = false, string explanation = null);


		/// <summary>
		/// Gets the cache key. By default returns <see cref="PanelInstanceCacheKey"/>, with default <see cref="PanelCacheKey.Expiration"/> (<see cref="PanelInstanceCacheKey.DefaultExpirationInSeconds"/> seconds)
		/// </summary>
		/// <returns></returns>
		public abstract PanelCacheKey CacheKey { get; }
	}

	/// <summary>
	/// Helper methods for <see cref="IPanelRenderer"/>
	/// </summary>
	public static class IPanelRendererExtensions
	{
		private static IImageCacheService _cache;

		/// <summary>
		/// Sets the cache service to use. Must be called before using GetCachedImage.
		/// </summary>
		/// <param name="cacheService">The cache service to use.</param>
		public static void SetCacheService(IImageCacheService cacheService)
		{
			_cache = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
		}

		/// <summary>
		/// Returns the number of cached images
		/// </summary>
		/// <returns></returns>
		public static int CacheEntries() => _cache?.Count() ?? -1;

		/// <summary>
		/// Gets the cached image.
		/// </summary>
		/// <param name="renderer">The renderer.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="colors">The colors.</param>
		/// <param name="log">The log.</param>
		/// <returns></returns>
		public static async Task<byte[]> GetCachedImage(this IPanelRenderer renderer, int width, int height, Color[] colors, IPanelRenderer.Log log)
		{
			if (_cache == null)
			{
				// Fallback to in-memory cache if not initialized
				_cache = new MemoryCacheService();
			}

			// Create cache key as string
			var imageCacheKey = new ImageCacheKey(
				panelCacheKey: renderer.CacheKey,
				imageSettings: new ImageSettings(width, height, colors));
			

			using (MiniProfiler.Current.Step($"Loading image from cache"))
			{
				var result = await _cache.GetOrCreateAsync(
					key: imageCacheKey,
					factory: async () =>
					{
						// Key not in cache, so get data.
						using (MiniProfiler.Current.Step($"Image not in cache, generating"))
						{
							var image = await renderer.GetImage(width, height, colors, log);
							using var stream = new MemoryStream();
							await image.SaveAsGifAsync(stream, encoder: new() { Quantizer = new PaletteQuantizer(colors) }); // When quantizer is not specified, colors are changed during saving as gif :|
							return stream.ToArray();
						}
					},
					expiration: imageCacheKey.PanelCacheKey.Expiration
				);

				return result;
			}
		}
	}

	/// <summary>
	/// Signature for a helper class for rendering a specific type of <see cref="Panel"/>
	/// </summary>
	public interface IPanelRenderer<in TPanel> : IPanelRenderer where TPanel : Panel
	{
		/// <summary>
		/// Configures the specified panel.
		/// </summary>
		/// <param name="panel">The panel.</param>
		void Configure(TPanel panel);
	}
}
