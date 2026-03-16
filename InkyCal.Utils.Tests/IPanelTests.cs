using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Xunit;
using Xunit.Sdk;

namespace InkyCal.Utils.Tests
{
	public abstract class IPanelTests<T>() where T : IPanelRenderer
	{

		protected abstract T GetRenderer();

		public static TheoryData<DisplayModel> DisplayModels()
		{
			return new TheoryData<DisplayModel>(Enum.GetValues<DisplayModel>());
		}

		[Theory]
		[MemberData(nameof(DisplayModels))]
		public virtual async Task GetImageTest(DisplayModel displayModel)
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
					throw SkipException.ForSkip(errorMessage);
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

					Console.WriteLine($"{actualColors.Count:n0} distinct colors in the image \n - {string.Join("\n - ", actualColors.Select(x => $"{x.Key} ({x.Value:n0})"))}), a palette of {colors.Length:n0} colors ({string.Join(",", colors.Select(x => x.ToString()))}) was specified.");
					Assert.False(extraActualColors.Any(), $"More or different colors than were requested were present in the image before saving: \n - {string.Join("\n - ", extraActualColors.Select(x => $"{x.Key:n0} ({x.Value:n0})"))}");
				}
				else if (image.GetType().IsGenericType)
					Console.WriteLine($"Image type is {image.GetType().Name}<{string.Join(",", image.GetType().GetGenericTypeDefinition().GenericTypeArguments.Select(x => x.Name))}>");
				else
					Console.WriteLine($"Image type is {image.GetType().Name}");


				using var fileStream = File.Create(filename);
				await image.SaveAsGifAsync(fileStream, encoder: new() { Quantizer = new PaletteQuantizer(colors) }, cancellationToken: TestContext.Current.CancellationToken);

			}

			bitmap = await Image.LoadAsync<Rgba32>(filename, TestContext.Current.CancellationToken);

			var fi = new FileInfo(filename);
			Assert.True(fi.Exists, $"File {fi.FullName} does not exist");

			Console.WriteLine(fi.FullName);

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
			Console.WriteLine(message);
			Assert.False(pixels.Count > colors.Length, message);
			Assert.False(extraColors.Any(), $"More or different colors than were requested were present in the saved image: \n - {string.Join("\n - ", extraColors.Select(x => $"{x.Key:n0} ({x.Value:n0})"))}");

		}
	}
}
