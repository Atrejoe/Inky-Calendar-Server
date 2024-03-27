// Ignore Spelling: Pdf Utils

using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using InkyCal.Models;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// 
	/// </summary>
	public class PdfRenderException : Exception
	{
		/// <inheritdoc/>
		public PdfRenderException() { }
		/// <inheritdoc/>
		public PdfRenderException(string message) : base(message) { }
		/// <inheritdoc/>
		public PdfRenderException(string message, Exception inner) : base(message, inner) { }
	}

	/// <summary>
	/// /
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="PanelRenderer{T}" />
	public abstract class PdfRenderer<T> : PanelRenderer<T> where T : Panel
	{

		/// <summary>
		/// 
		/// </summary>
		protected PdfRenderer():base() { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="panel"></param>
		protected PdfRenderer(T panel) : base(panel) { }

		private static readonly MemoryCache _cache = new (new MemoryCacheOptions()
		{
			SizeLimit = 1024 * 1024 * 500,
		});

		/// <summary>
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		/// <param name="log"></param>
		/// <returns></returns>
		/// <inheritdoc />
		public override async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
		{
			//Get pdf as byte array
			var pdf = await GetPDF();

			Image<Rgba32> image;

			using (MiniProfiler.Current.Step($"Loading converted Pdf from cache"))

				if (_cache.TryGetValue(CacheKey, out byte[] bytes))
					image = Image.Load<Rgba32>(bytes);

				else
				{
					using (MiniProfiler.Current.Step($"Converted pdf not in cache, generating"))
					{
						var settings = new MagickReadSettings
						{
							// Settings the density to 300 dpi will create an image with a better quality
							Density = new Density(300),
							Format = MagickFormat.Pdf,
							Verbose = true,
						};

						using var images = new MagickImageCollection();
						// Add all the pages of the pdf file to the collection
						images.Read(pdf, settings);

						if (images.Count == 0)
							throw new PdfRenderException($"{pdf.Length:n0} byte pdf file resulted in 0 images. Is Ghostscript installed?");

						using var ms = new MemoryStream();

						// Create new image that appends all the pages horizontally
						using var vertical = images.AppendVertically();

						// Save result as a png
						vertical.Write(ms, MagickFormat.Png);

						ms.Position = 0;

						//Load PNG
						image = Image.Load<Rgba32>(ms);

						var cacheEntryOptions = new MemoryCacheEntryOptions()
							.SetSize(ms.Length)
							// Remove from cache after this time, regardless of sliding expiration
							.SetAbsoluteExpiration(CacheKey.Expiration);

						// Save data in cache.
						using (MiniProfiler.Current.Step($"Storing converted Pdf ({ms.Length:n0} bytes) in cache until {DateTime.Now.Add(CacheKey.Expiration)}"))
							_cache.Set(CacheKey, ms.ToArray(), cacheEntryOptions);
					}
				}

			using (MiniProfiler.Current.Step($"Converting hi-res image to correct resolution and color palette"))
				image.Mutate(x => x
					.EntropyCrop()
					.Resize(new ResizeOptions() { Mode = ResizeMode.Crop, Size = new Size(width, height), Position = AnchorPositionMode.TopLeft })
					//.BackgroundColor(Color.Transparent)
					.Quantize(new PaletteQuantizer(colors)));


			return image;
		}

		/// <summary>
		/// Gets the Pdf file as binary
		/// </summary>
		/// <returns></returns>
		protected abstract Task<byte[]> GetPDF();
	}
}
