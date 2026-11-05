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
	/// Tests <see cref="OpenAICompatibleImageService"/> through OpenAI's official SDK against a local stub.
	/// </summary>
	public class OpenAICompatibleImageServiceTests
	{
		private static readonly byte[] PngBytes = StubHttpServer.TinyPng();
		private static readonly string Png = Convert.ToBase64String(PngBytes);

		private static OpenAICompatibleOptions Options(StubHttpServer server) => new()
		{
			Endpoint = new Uri(server.BaseAddress, "v1"),
			ImageModel = "image-model",
			ChatModel = "chat-model",
		};

		[Fact]
		public async Task GetChatCompletionAsync_SendsMessages_ReturnsText()
		{
			using var server = new StubHttpServer(_ => new(
				@"{""id"":""1"",""object"":""chat.completion"",""created"":1,""model"":""chat-model"",""choices"":[{""index"":0,""finish_reason"":""stop"",""message"":{""role"":""assistant"",""content"":""An engraving""}}]}"));
			using var sut = new OpenAICompatibleImageService(Options(server) with { ApiKey = "key" });

			var result = await sut.GetChatCompletionAsync("system", "user", CancellationToken.None);

			Assert.Equal("An engraving", result);
			var request = Assert.Single(server.Requests);
			Assert.Equal("/v1/chat/completions", request.PathAndQuery);
			Assert.Equal("Bearer key", request.Headers["Authorization"]);
			using var json = JsonDocument.Parse(request.Body);
			Assert.Equal("chat-model", json.RootElement.GetProperty("model").GetString());
			var messages = json.RootElement.GetProperty("messages");
			Assert.Equal("system", messages[0].GetProperty("role").GetString());
			Assert.Equal("user", messages[1].GetProperty("role").GetString());
		}

		[Fact]
		public async Task GenerateImageAsync_Base64Response_OnlySendsConfiguredParameters()
		{
			using var server = new StubHttpServer(_ => new($@"{{""created"":1,""data"":[{{""b64_json"":""{Png}""}}]}}"));
			using var sut = new OpenAICompatibleImageService(Options(server));

			using var stream = await sut.GenerateImageAsync("a prompt", CancellationToken.None);
			using var image = await Image.LoadAsync<Rgba32>(stream);

			Assert.Equal(2, image.Width);
			var request = Assert.Single(server.Requests);
			Assert.Equal("/v1/images/generations", request.PathAndQuery);
			using var json = JsonDocument.Parse(request.Body);
			Assert.Equal("image-model", json.RootElement.GetProperty("model").GetString());
			Assert.Equal("a prompt", json.RootElement.GetProperty("prompt").GetString());
			Assert.False(json.RootElement.TryGetProperty("quality", out _));
			Assert.False(json.RootElement.TryGetProperty("size", out _));
			Assert.False(json.RootElement.TryGetProperty("response_format", out _));
		}

		[Fact]
		public async Task GenerateImageAsync_SendsQualitySizeAndResponseFormat()
		{
			using var server = new StubHttpServer(_ => new($@"{{""created"":1,""data"":[{{""b64_json"":""{Png}""}}]}}"));
			using var sut = new OpenAICompatibleImageService(Options(server) with { Quality = "low", Size = (1024, 768), RequestBase64 = true });

			using var _ = await sut.GenerateImageAsync("a prompt", CancellationToken.None);

			using var json = JsonDocument.Parse(server.Requests.Single().Body);
			Assert.Equal("low", json.RootElement.GetProperty("quality").GetString());
			Assert.Equal("1024x768", json.RootElement.GetProperty("size").GetString());
			Assert.Equal("b64_json", json.RootElement.GetProperty("response_format").GetString());
		}

		[Fact]
		public async Task GenerateImageAsync_UrlResponse_DownloadsImage()
		{
			StubHttpServer server = null;
			server = new StubHttpServer(r => r.PathAndQuery == "/image.png"
				? new(null, "image/png") { Bytes = PngBytes }
				: new($@"{{""created"":1,""data"":[{{""url"":""{server.BaseAddress}image.png""}}]}}"));
			using (server)
			using (var sut = new OpenAICompatibleImageService(Options(server)))
			{
				using var stream = await sut.GenerateImageAsync("a prompt", CancellationToken.None);
				using var image = await Image.LoadAsync<Rgba32>(stream);

				Assert.Equal(2, image.Height);
				Assert.Equal(2, server.Requests.Count);
			}
		}

		[Theory]
		[InlineData(null, null, false)]
		[InlineData("key", null, true)]
		[InlineData(null, "http://localai:8080/v1", true)]
		public void IsAvailable_RequiresKeyOrEndpoint(string apiKey, string endpoint, bool expected)
		{
			using var sut = new OpenAICompatibleImageService(new OpenAICompatibleOptions
			{
				ApiKey = apiKey,
				Endpoint = endpoint is null ? null : new Uri(endpoint),
				ImageModel = "image",
				ChatModel = "chat",
			});

			Assert.Equal(expected, sut.IsAvailable);
		}

		[Fact]
		public void IsAvailable_RequiresModels()
		{
			using var sut = new OpenAICompatibleImageService(new OpenAICompatibleOptions { ApiKey = "key" });

			Assert.False(sut.IsAvailable);
		}

		[Fact]
		public void OpenAIService_UsesMediumQualityGptImage2()
		{
			using var sut = new OpenAIService("key");

			Assert.True(sut.IsAvailable);
			Assert.Equal("gpt-image-2", sut.Options.ImageModel);
			Assert.Equal("medium", sut.Options.Quality);
			Assert.False(sut.Options.RequestBase64);
			Assert.Null(sut.Options.Endpoint);
		}

		[Theory]
		[InlineData("1024x768", 1024, 768)]
		[InlineData("512X512", 512, 512)]
		public void ParseSize_Valid(string size, int width, int height)
			=> Assert.Equal((width, height), OpenAICompatibleOptions.ParseSize(size));

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("big")]
		[InlineData("0x100")]
		public void ParseSize_Invalid_ReturnsNull(string size)
			=> Assert.Null(OpenAICompatibleOptions.ParseSize(size));
	}
}
