using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using InkyCal.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.Primitives;

namespace InkyCal.Utils
{
	/// <summary>
	/// /
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="InkyCal.Utils.PanelRenderer{T}" />
	public abstract class PdfRenderer<T> : PanelRenderer<T> where T : Panel {

		/// <summary>
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		/// <param name="log"></param>
		/// <returns></returns>
		/// <inheritdoc />
		public override async Task<Image> GetImage(int width, int height, SixLabors.ImageSharp.Color[] colors, IPanelRenderer.Log log)
		{
			var pdf = await GetPdf();

			var settings = new MagickReadSettings
			{
				// Settings the density to 300 dpi will create an image with a better quality
				Density = new Density(300),
				Format = MagickFormat.Pdf
			};


			using (var images = new MagickImageCollection())
			{
				// Add all the pages of the pdf file to the collection
				images.Read(pdf, settings);

				using var ms = new MemoryStream();
				// Create new image that appends all the pages horizontally
				using (var horizontal = images.AppendHorizontally())
				{
					// Save result as a png
					horizontal.Write(ms, MagickFormat.Png);
				}
				ms.Position = 0;

				//Load PNG
				var image = Image.Load(ms, new PngDecoder());
				var colorsExtended = new List<SixLabors.ImageSharp.Color>(colors);
				//foreach (float i in Enumerable.Range(0, 10))
				//	colorsExtended.Add(Color.Black.WithAlpha(i / 10));

				//colorsExtended.Add(Color.Gray);
				//colorsExtended.Add(Color.DarkGray);
				//colorsExtended.Add(Color.DarkSlateGray);
				//colorsExtended.Add(Color.DimGray);
				//colorsExtended.Add(Color.LightGray);
				//colorsExtended.Add(Color.LightSlateGray);
				//colorsExtended.Add(Color.SlateGray);

				image.Mutate(x => x
					.Resize(new ResizeOptions() { Mode = ResizeMode.Max, Size = new Size(width, height) })
					.BackgroundColor(SixLabors.ImageSharp.Color.Transparent)
					.Quantize(new PaletteQuantizer(colorsExtended.ToArray(), dither: true))
					)
					;
				return image;
			}
		}

		/// <summary>
		/// Gets the Pdf file as binary
		/// </summary>
		/// <returns></returns>
		protected abstract Task<byte[]> GetPdf();
	}
}
