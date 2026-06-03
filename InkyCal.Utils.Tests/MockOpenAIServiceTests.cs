using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	public class MockOpenAIServiceTests(ITestOutputHelper output)
	{
		private readonly MockOpenAIService _sut = new();

		[Fact]
		public void IsAvailable_ReturnsTrue()
			=> Assert.True(_sut.IsAvailable);

		[Theory]
		[InlineData("system prompt", "user prompt")]
		[InlineData("", "")]
		public async Task GetChatCompletionAsync_ReturnsNonEmptyString(string systemPrompt, string userPrompt)
		{
			var result = await _sut.GetChatCompletionAsync(systemPrompt, userPrompt, CancellationToken.None);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			output.WriteLine(result);
		}

		[Theory]
		[InlineData("a simple prompt")]
		[InlineData("a very long prompt with many calendar events and various colors specified")]
		[InlineData("")]
		public async Task GenerateImageAsync_Returns1000x1000Image(string prompt)
		{
			using var stream = await _sut.GenerateImageAsync(prompt, CancellationToken.None);

			Assert.NotNull(stream);
			Assert.True(stream.CanRead);

			using var image = await Image.LoadAsync<Rgba32>(stream);

			output.WriteLine($"Image size: {image.Width}×{image.Height}");
			Assert.Equal(1000, image.Width);
			Assert.Equal(1000, image.Height);
		}

		[Fact]
		public async Task GenerateImageAsync_ImageHasTextRendered()
		{
			using var stream = await _sut.GenerateImageAsync("test prompt", CancellationToken.None);
			using var image = await Image.LoadAsync<Rgba32>(stream);

			var nonWhitePixels = Enumerable.Range(0, image.Width)
				.SelectMany(x => Enumerable.Range(0, image.Height).Select(y => image[x, y]))
				.Count(p => p.R < 255 || p.G < 255 || p.B < 255);

			output.WriteLine($"{nonWhitePixels:n0} non-white pixels out of {image.Width * image.Height:n0} total");
			Assert.True(nonWhitePixels > 0, "Expected text to be rendered on the placeholder image.");
		}
	}
}
