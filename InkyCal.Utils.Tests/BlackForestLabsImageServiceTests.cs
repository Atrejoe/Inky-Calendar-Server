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
	/// Tests <see cref="BlackForestLabsImageService"/> through the Kiota-generated client against a local stub.
	/// </summary>
	public class BlackForestLabsImageServiceTests
	{
		private static readonly byte[] PngBytes = StubHttpServer.TinyPng();

		private sealed class FakePromptService(bool available) : IImageGenerationService
		{
			public bool IsAvailable => available;
			public SixLabors.ImageSharp.Processing.ResizeMode ResizeMode => default;
			public Task<string> GetChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken) => Task.FromResult("composed prompt");
			public Task<System.IO.Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken) => throw new NotSupportedException();
		}

		private static BlackForestLabsImageService Create(StubHttpServer server, string model = null, IChatCompletionService promptService = null)
			=> new("key", model, 1000, 600, promptService, server?.BaseAddress, TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(10));

		/// <summary>Simulates BFL: submit, poll (pending, then ready), download sample.</summary>
		private static StubHttpServer BflStub(string finalStatus = "Ready")
		{
			StubHttpServer server = null;
			var polls = 0;
			server = new StubHttpServer(r =>
			{
				if (r.Method == "POST")
					return new($@"{{""id"":""task-1"",""polling_url"":""{server.BaseAddress}v1/get_result?id=task-1"",""cost"":1.4}}");
				if (r.PathAndQuery.StartsWith("/v1/get_result", StringComparison.Ordinal))
					return Interlocked.Increment(ref polls) == 1
						? new(@"{""id"":""task-1"",""status"":""Pending"",""progress"":0.5}")
						: new($@"{{""id"":""task-1"",""status"":""{finalStatus}"",""result"":{{""sample"":""{server.BaseAddress}sample.jpg"",""prompt"":""x""}}}}");
				if (r.PathAndQuery == "/sample.jpg")
					return new(null, "image/png") { Bytes = PngBytes };
				return new("not found", "text/plain", 404);
			});
			return server;
		}

		[Fact]
		public async Task GenerateImageAsync_SubmitsPollsAndDownloads()
		{
			using var server = BflStub();
			using var sut = Create(server);

			using var stream = await sut.GenerateImageAsync("a prompt", CancellationToken.None);
			using var image = await Image.LoadAsync<Rgba32>(stream);

			Assert.Equal(2, image.Width);
			var requests = server.Requests.ToArray();
			Assert.Equal(["POST /v1/flux-2-klein-9b", "GET /v1/get_result?id=task-1", "GET /v1/get_result?id=task-1", "GET /sample.jpg"],
				requests.Select(r => $"{r.Method} {r.PathAndQuery}"));

			var submit = requests[0];
			Assert.Equal("key", submit.Headers["x-key"]);
			using var json = JsonDocument.Parse(submit.Body);
			Assert.Equal("a prompt", json.RootElement.GetProperty("prompt").GetString());
			// Rounded to multiples of 16
			Assert.Equal(1008, json.RootElement.GetProperty("width").GetInt32());
			Assert.Equal(608, json.RootElement.GetProperty("height").GetInt32());
			Assert.Equal("jpeg", json.RootElement.GetProperty("output_format").GetString());

			Assert.Equal("key", requests[1].Headers["x-key"]);
			Assert.False(requests[3].Headers.ContainsKey("x-key"), "The api key must not be sent to the sample (delivery) url");
		}

		[Theory]
		[InlineData("flux-2-klein-4b")]
		[InlineData("flux-2-pro")]
		[InlineData("flux-2-flex")]
		[InlineData("FLUX-2-MAX")]
		public async Task GenerateImageAsync_PostsToModelEndpoint(string model)
		{
			using var server = BflStub();
			using var sut = Create(server, model);

			using var _ = await sut.GenerateImageAsync("a prompt", CancellationToken.None);

			Assert.Equal($"/v1/{model.ToLowerInvariant()}", server.Requests.First().PathAndQuery);
		}

		[Theory]
		[InlineData("Content Moderated")]
		[InlineData("Error")]
		public async Task GenerateImageAsync_FailedStatus_Throws(string status)
		{
			using var server = BflStub(status);
			using var sut = Create(server);

			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GenerateImageAsync("a prompt", CancellationToken.None));
			Assert.Contains("failed with status", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public async Task GenerateImageAsync_PollingUrlOnUntrustedHost_DoesNotSendKey()
		{
			using var server = new StubHttpServer(_ => new(@"{""id"":""task-1"",""polling_url"":""http://localhost:1/v1/get_result?id=task-1""}"));
			using var sut = Create(server);

			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GenerateImageAsync("a prompt", CancellationToken.None));
			Assert.Contains("Refusing to send", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void UnsupportedModel_Throws()
			=> Assert.Throws<NotSupportedException>(() => Create(null, "flux-dev"));

		[Fact]
		public async Task GetChatCompletionAsync_DelegatesToPromptService()
		{
			using var sut = Create(null, promptService: new FakePromptService(available: true));

			Assert.Equal("composed prompt", await sut.GetChatCompletionAsync("system", "user", CancellationToken.None));
		}

		[Fact]
		public async Task GetChatCompletionAsync_PromptServiceUnavailable_PassesPromptThrough()
		{
			using var sut = Create(null, promptService: new FakePromptService(available: false));

			var result = await sut.GetChatCompletionAsync("system", "user", CancellationToken.None);

			Assert.StartsWith("system", result, StringComparison.Ordinal);
			Assert.EndsWith("user", result, StringComparison.Ordinal);
		}

		[Theory]
		[InlineData("1024x768", 1024, 768)]
		[InlineData("512X512", 512, 512)]
		public void ParseSize_Valid(string size, int width, int height)
			=> Assert.Equal((width, height), ImageGenerationServiceFactory.ParseSize(size));

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("big")]
		public void ParseSize_Invalid_ReturnsNull(string size)
			=> Assert.Null(ImageGenerationServiceFactory.ParseSize(size));
	}
}
