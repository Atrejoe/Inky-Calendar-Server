using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using InkyCal.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace InkyCal.Utils
{
	/// <summary>
	/// /
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="InkyCal.Utils.PanelRenderer{T}" />
	public abstract class PdfRenderer<T> : PanelRenderer<T> where T : Panel
	{

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
			var pdf = await GetPdf();

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
				throw new System.Exception($"{pdf.Length:n0} byte pdf file resulted in 0 images. Is Ghostscript installed?");

			using var ms = new MemoryStream();

			// Create new image that appends all the pages horizontally
			using var vertical = images.AppendVertically();

			// Save result as a png
			vertical.Write(ms, MagickFormat.Png);

			ms.Position = 0;


			//Load PNG
			var image = Image.Load(ms, new PngDecoder());
			
			var colorsExtended = new List<Color>(colors);

			image.Mutate(x => x
				.EntropyCrop()
				.Resize(new ResizeOptions() { Mode = ResizeMode.Crop, Size = new Size(width, height), Position = AnchorPositionMode.TopLeft })
				.BackgroundColor(Color.Transparent)
				.Quantize(
					new PaletteQuantizer(colorsExtended.ToArray(),
					new QuantizerOptions()
					{
						Dither = KnownDitherings.FloydSteinberg
					}
				))
				);

			return image;
		}

		/// <summary>
		/// Gets the Pdf file as binary
		/// </summary>
		/// <returns></returns>
		protected abstract Task<byte[]> GetPdf();
	}
}
