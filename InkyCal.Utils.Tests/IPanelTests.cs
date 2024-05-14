using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	public abstract class IPanelTests<T> where T : IPanelRenderer
	{

		protected readonly ITestOutputHelper output;

		protected IPanelTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		protected abstract T GetRenderer();

		[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Method for delivery test data")]
		public static IEnumerable<object[]> DisplayModels()
		{
			return Enum.GetValues(typeof(DisplayModel))
				.Cast<DisplayModel>()
				.Select(x => new object[] { x });
		}

		[SkippableTheory]
		[MemberData(nameof(DisplayModels))]
		public async virtual Task GetImageTest(DisplayModel displayModel)
		{
			//arrange
			var panel = GetRenderer();
			var filename = $"GetImageTest_{typeof(T).Name}_{displayModel}.gif";
			displayModel.GetSpecs(out var width, out var height, out var colors);

			IPanelRenderer.Log assertHandledOnly = (Exception ex, bool handled, string explanation) =>
			{
				if (handled)
				{
					var errorMessage = $"{explanation ?? "Handled exception"}: {ex.Message}";
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

			Image<Rgba32> bitmap;

			using (var image = await panel.GetImage(
								width: width,
								height: height,
								colors: colors,
								assertHandledOnly))
			{

				//assert
				Assert.NotNull(image);

				if (image is Image<Rgba32> transparentImage)
				{
					var actualColors = Enumerable.Range(0, transparentImage.Width - 1)
						.SelectMany(x =>
						{
							return Enumerable.Range(0, image.Height - 1).Select(y => transparentImage[x, y]);
						})
						.Select(x => x.ToHex())
						.GroupBy(x => x)
						.ToDictionary(x => x.Key, x => x.Count());

					var extraActualColors = actualColors
									.Where(x => !colors.Select(x => x.ToHex()).Contains(x.Key));

					output.WriteLine($"{actualColors.Count:n0} distinct colors in the image \n - {string.Join("\n - ", actualColors.Select(x => $"{x.Key} ({x.Value:n0})"))}), a palette of {colors.Length:n0} colors ({string.Join(",", colors.Select(x => x.ToString()))}) was specified.");
					Assert.False(extraActualColors.Any(), $"More or different colors than were requested were present in the image before saving: \n - {string.Join("\n - ", extraActualColors.Select(x => $"{x.Key:n0} ({x.Value:n0})"))}");
				}
				else if (image.GetType().IsGenericType)
					output.WriteLine($"Image type is {image.GetType().Name}<{string.Join(",", image.GetType().GetGenericTypeDefinition().GenericTypeArguments.Select(x => x.Name))}>");
				else
					output.WriteLine($"Image type is {image.GetType().Name}");


				using var fileStream = File.Create(filename);
				await image.SaveAsGifAsync(fileStream, encoder: new () {  Quantizer = new PaletteQuantizer(colors) });

			}

			bitmap = await Image.LoadAsync<Rgba32>(filename);

			var fi = new FileInfo(filename);
			Assert.True(fi.Exists, $"File {fi.FullName} does not exist");

			output.WriteLine(fi.FullName);

			var pixels = Enumerable.Range(0, bitmap.Width - 1)
				.SelectMany(x =>
				{
					return Enumerable.Range(0, bitmap.Height - 1).Select(y => bitmap[x, y]);
				})
				.Select(x => x.ToHex())
				.GroupBy(x => x)
				.ToDictionary(x => x.Key, x => x.Count());

			var extraColors = pixels
								.Where(x => !colors.Select(x => x.ToHex()).Contains(x.Key));

			var message = $"{pixels.Count:n0} distinct colors in the image \n - {string.Join("\n - ", pixels.Select(x => $"{x.Key} ({x.Value:n0})"))}), a palette of {colors.Length:n0} colors ({string.Join(",", colors.Select(x => x.ToString()))}) was specified.";
			output.WriteLine(message);
			Assert.False(pixels.Count > colors.Length, message);
			Assert.False(extraColors.Any(), $"More or different colors than were requested were present in the saved image: \n - {string.Join("\n - ", extraColors.Select(x => $"{x.Key:n0} ({x.Value:n0})"))}");

		}
	}
}
