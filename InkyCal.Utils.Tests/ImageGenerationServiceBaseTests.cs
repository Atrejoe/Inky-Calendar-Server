using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InkyCal.Utils.Tests
{
	public class ImageGenerationServiceBaseTests
	{
		private sealed class FakeService(Func<string, Task<byte[]>> generate) : ImageGenerationServiceBase(imagesPerWindow: 100)
		{
			public int Concurrent;
			public int MaxConcurrent;

			public override string ProviderName => "Fake";
			public override bool IsAvailable => true;

			protected override Task<string> GetChatCompletionCoreAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
				=> Task.FromResult($"{systemPrompt}|{userPrompt}");

			protected override async Task<byte[]> GenerateImageCoreAsync(string prompt, CancellationToken cancellationToken)
			{
				var current = Interlocked.Increment(ref Concurrent);
				MaxConcurrent = Math.Max(MaxConcurrent, current);
				try
				{
					return await generate(prompt);
				}
				finally
				{
					Interlocked.Decrement(ref Concurrent);
				}
			}
		}

		[Fact]
		public async Task GenerateImageAsync_ReturnsProviderBytes()
		{
			using var sut = new FakeService(_ => Task.FromResult(new byte[] { 1, 2, 3 }));

			using var stream = await sut.GenerateImageAsync("prompt", CancellationToken.None);
			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms);

			Assert.Equal(new byte[] { 1, 2, 3 }, ms.ToArray());
		}

		[Theory]
		[InlineData(null)]
		[InlineData(new byte[0])]
		public async Task GenerateImageAsync_NoImageData_Throws(byte[] result)
		{
			using var sut = new FakeService(_ => Task.FromResult(result));

			await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GenerateImageAsync("prompt", CancellationToken.None));
		}

		[Fact]
		public async Task GenerateImageAsync_SerialisesRequests()
		{
			using var sut = new FakeService(async _ => { await Task.Delay(20); return [1]; });

			await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => sut.GenerateImageAsync("prompt", CancellationToken.None)));

			Assert.Equal(1, sut.MaxConcurrent);
		}

		[Fact]
		public async Task GetChatCompletionAsync_DelegatesToProvider()
		{
			using var sut = new FakeService(_ => Task.FromResult(new byte[] { 1 }));

			Assert.Equal("a|b", await sut.GetChatCompletionAsync("a", "b", CancellationToken.None));
		}

		[Theory]
		[InlineData("mock", typeof(MockImageGenerationService))]
		[InlineData("Mock", typeof(MockImageGenerationService))]
		[InlineData("OpenAI", typeof(OpenAIService))]
		[InlineData("Gemini", typeof(GoogleGeminiImageService))]
		[InlineData("", typeof(OpenAIService))]
		[InlineData(null, typeof(OpenAIService))]
		public void Factory_CreatesConfiguredProvider(string provider, Type expected)
			=> Assert.IsType(expected, ImageGenerationServiceFactory.Create(provider));

		[Fact]
		public void Factory_UnknownProvider_Throws()
			=> Assert.Throws<NotSupportedException>(() => ImageGenerationServiceFactory.Create("does-not-exist"));
	}
}
