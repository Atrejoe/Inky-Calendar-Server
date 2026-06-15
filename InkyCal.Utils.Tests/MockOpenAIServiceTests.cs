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
		[InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt.")]
		public async Task GenerateImageAsync_ReturnsValidImage(string prompt)
		{
			using var stream = await _sut.GenerateImageAsync(prompt, CancellationToken.None);

			Assert.NotNull(stream);
			Assert.True(stream.CanRead);

			using var image = await Image.LoadAsync<Rgba32>(stream);

			output.WriteLine($"Image size: {image.Width}×{image.Height}");
			Assert.True(image.Width > 0);
			Assert.True(image.Height > 0);
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

		[Fact]
		public async Task GenerateImageAsync_LongPromptProducesTallerImage()
		{
			using var shortStream = await _sut.GenerateImageAsync("hi", CancellationToken.None);
			using var longStream = await _sut.GenerateImageAsync(new string('A', 2000), CancellationToken.None);

			using var shortImage = await Image.LoadAsync<Rgba32>(shortStream);
			using var longImage = await Image.LoadAsync<Rgba32>(longStream);

			output.WriteLine($"Short prompt → {shortImage.Width}×{shortImage.Height}");
			output.WriteLine($"Long prompt  → {longImage.Width}×{longImage.Height}");

			Assert.True(longImage.Height > shortImage.Height,
				"A longer prompt should produce a taller image.");
		}
	}
}
