using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see cref="GoogleGeminiImageService"/> through the real <c>Google.GenAI</c> client against a local stub.
	/// </summary>
	public class GoogleGeminiImageServiceTests
	{
		private static readonly string Png = Convert.ToBase64String(StubHttpServer.TinyPng());

		[Fact]
		public void IsAvailable_RequiresApiKey()
		{
			using var withKey = new GoogleGeminiImageService("key");
			using var withoutKey = new GoogleGeminiImageService(null);

			Assert.True(withKey.IsAvailable);
			Assert.False(withoutKey.IsAvailable);
		}

		[Fact]
		public async Task GetChatCompletionAsync_SendsSystemInstruction_ReturnsText()
		{
			using var server = new StubHttpServer(_ => new(@"{""candidates"":[{""content"":{""role"":""model"",""parts"":[{""text"":""An engraving of a dentist""}]}}]}"));
			using var sut = new GoogleGeminiImageService("key", null, "chat-model", null, server.BaseAddress);

			var result = await sut.GetChatCompletionAsync("system", "user", CancellationToken.None);

			Assert.Equal("An engraving of a dentist", result);
			var request = Assert.Single(server.Requests);
			Assert.Contains("chat-model:generateContent", request.PathAndQuery, StringComparison.Ordinal);
			Assert.Equal("key", request.Headers["x-goog-api-key"]);
			using var json = JsonDocument.Parse(request.Body);
			Assert.Equal("system", json.RootElement.GetProperty("systemInstruction").GetProperty("parts")[0].GetProperty("text").GetString());
			Assert.Equal("user", json.RootElement.GetProperty("contents")[0].GetProperty("parts")[0].GetProperty("text").GetString());
		}

		[Fact]
		public async Task GenerateImageAsync_GeminiImageModel_UsesGenerateContent()
		{
			using var server = new StubHttpServer(_ => new(
				$@"{{""candidates"":[{{""content"":{{""role"":""model"",""parts"":[{{""text"":""here you go""}},{{""inlineData"":{{""mimeType"":""image/png"",""data"":""{Png}""}}}}]}}}}]}}"));
			using var sut = new GoogleGeminiImageService("key", "gemini-test-image", null, "16:9", server.BaseAddress);

			using var stream = await sut.GenerateImageAsync("prompt", CancellationToken.None);
			using var image = await Image.LoadAsync<Rgba32>(stream);

			Assert.Equal(2, image.Width);
			var request = Assert.Single(server.Requests);
			Assert.Contains("gemini-test-image:generateContent", request.PathAndQuery, StringComparison.Ordinal);
			using var json = JsonDocument.Parse(request.Body);
			var config = json.RootElement.GetProperty("generationConfig");
			Assert.Equal("IMAGE", config.GetProperty("responseModalities")[0].GetString());
			Assert.Equal("16:9", config.GetProperty("imageConfig").GetProperty("aspectRatio").GetString());
		}

		[Fact]
		public async Task GenerateImageAsync_NoImageReturned_Throws()
		{
			using var server = new StubHttpServer(_ => new(@"{""candidates"":[{""content"":{""role"":""model"",""parts"":[{""text"":""I cannot draw that""}]}}]}"));
			using var sut = new GoogleGeminiImageService("key", null, null, null, server.BaseAddress);

			await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GenerateImageAsync("prompt", CancellationToken.None));
		}
	}
}
