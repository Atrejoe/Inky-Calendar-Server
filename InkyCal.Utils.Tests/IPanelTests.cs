using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.ImageSharp.Formats.Png;
using Xunit;

namespace InkyCal.Utils.Tests
{
	public abstract class IPanelTests<T> where T : IPanelRenderer
	{

		protected abstract T GetRenderer();

		[SkippableFact]
		public async Task GetImageTest()
		{
			//arrange
			var panel = GetRenderer();
			var filename = $"GetImageTest_{typeof(T).Name}.png";
			DisplayModel.epd_7_in_5_v2_colour.GetSpecs(out var width, out var height, out var colors);

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

			//assert
			Assert.NotNull(image);

			using var fileStream = File.Create(filename);
			image.Save(fileStream, new PngEncoder());

			var fi = new FileInfo(filename);
			Assert.True(fi.Exists, $"File {fi.FullName} does not exist");

			Trace.WriteLine(fi.FullName);

		}
	}
}
