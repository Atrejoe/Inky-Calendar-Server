using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.ImageSharp.Formats.Png;
using Xunit;

namespace InkyCal.Utils.Tests
{
	public abstract class IPanelTests<T> where T : IPanelRenderer
	{

		protected abstract T GetRenderer();

		[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Method for delivery test data")]
		public static IEnumerable<object[]> DisplayModels() {
			return Enum.GetValues(typeof(DisplayModel))
				.Cast<DisplayModel>()
				.Select(x => new object[] { x });
		}

		[Theory]
		[MemberData(nameof(DisplayModels))]
		public async Task GetImageTest(DisplayModel displayModel)
		{
			//arrange
			//const DisplayModel displayModel = DisplayModel.epd_7_in_5_v3_colour;
			var panel = GetRenderer();
			var filename = $"GetImageTest_{typeof(T).Name}_{displayModel}.png";
			displayModel.GetSpecs(out var width, out var height, out var colors);

			IPanelRenderer.Log assertHandledOnly = (Exception ex, bool handled, string explanation) =>
			{
				if (handled)
				{
					var errorMessage = $"{explanation ?? "Handled exception" }: {ex.Message}";
					Console.WriteLine(errorMessage);
					throw new SkipException(errorMessage);
				}
				else
				{
					if (!string.IsNullOrEmpty(explanation))
						Console.Error.WriteLine(explanation);
					throw ex;
				}
			};

			//act
			var image = await panel.GetImage(
								width: height,
								height: width,
								colors: colors,
								assertHandledOnly);

			var bitmap = image.CloneAs<SixLabors.ImageSharp.PixelFormats.Rgba32>();

			//assert
			Assert.NotNull(image);

			var pixels = Enumerable.Range(0, bitmap.Width - 1)
				.SelectMany(x =>
				{
					return Enumerable.Range(0, bitmap.Height - 1).Select(y => bitmap[x, y]);
				}).ToHashSet();

			Trace.WriteLine($"{pixels.Count:n0} distinct colors in the image, a palette of {colors.Length:n0} colors was specified.");
			Assert.False(pixels.Count > colors.Length, $"{pixels.Count:n0} distinct colors in the image, while a palette of {colors.Length:n0} was specified.");

			using var fileStream = File.Create(filename);
			image.Save(fileStream, new PngEncoder());

			var fi = new FileInfo(filename);
			Assert.True(fi.Exists, $"File {fi.FullName} does not exist");

			Trace.WriteLine(fi.FullName);

		}
	}
}
