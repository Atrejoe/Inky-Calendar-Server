using System;
using System.ClientModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;

namespace InkyCal.Utils
{
	/// <summary>
	/// Generates images through any API that implements OpenAI's <c>/v1/images/generations</c> and <c>/v1/chat/completions</c>,
	/// using OpenAI's own .NET SDK (<c>OpenAI</c> NuGet package).
	/// </summary>
	/// <remarks>
	/// Known compatible providers:
	/// <list type="bullet">
	/// <item>OpenAI itself (no endpoint), see <see cref="OpenAIService"/>.</item>
	/// <item>Together AI, <c>https://api.together.xyz/v1</c>, e.g. FLUX.1 [schnell] / FLUX.2 models.</item>
	/// <item>LocalAI (local, self-hosted), e.g. <c>http://localai:8080/v1</c> with a stable-diffusion.cpp or diffusers FLUX/SDXL model.</item>
	/// <item>Ollama (local, experimental image generation), <c>http://ollama:11434/v1</c>.</item>
	/// </list>
	/// </remarks>
	public class OpenAICompatibleImageService : ImageGenerationServiceBase
	{
		private static readonly HttpClient _downloadClient = new();

		private readonly OpenAICompatibleOptions _options;
		private readonly Lazy<OpenAIClient> _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenAICompatibleImageService"/> class.
		/// </summary>
		public OpenAICompatibleImageService(OpenAICompatibleOptions options)
		{
			ArgumentNullException.ThrowIfNull(options);
			_options = options;

			_client = new Lazy<OpenAIClient>(() => new OpenAIClient(
				// Local servers usually do not require a key, the SDK does.
				new ApiKeyCredential(string.IsNullOrEmpty(options.ApiKey) ? "not-required" : options.ApiKey),
				new OpenAIClientOptions { Endpoint = options.Endpoint }));
		}

		/// <summary>The options this service was created with.</summary>
		internal OpenAICompatibleOptions Options => _options;

		/// <inheritdoc/>
		public override string ProviderName => _options.ProviderName ?? _options.Endpoint?.Host ?? "OpenAI";

		/// <summary>
		/// Available when an API key is configured or, for local servers that do not need one, an endpoint.
		/// The image and chat model must always be set.
		/// </summary>
		public override bool IsAvailable
			=> (!string.IsNullOrEmpty(_options.ApiKey) || _options.Endpoint is not null)
			&& !string.IsNullOrEmpty(_options.ImageModel)
			&& !string.IsNullOrEmpty(_options.ChatModel);

		/// <inheritdoc/>
		protected override async Task<string> GetChatCompletionCoreAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
		{
			var chat = _client.Value.GetChatClient(_options.ChatModel);
			ChatCompletion completion = await chat.CompleteChatAsync(
				[new SystemChatMessage(systemPrompt), new UserChatMessage(userPrompt)],
				cancellationToken: cancellationToken);

			return string.Concat(completion.Content.Where(c => c.Kind == ChatMessageContentPartKind.Text).Select(c => c.Text));
		}

		/// <inheritdoc/>
		protected override async Task<byte[]> GenerateImageCoreAsync(string prompt, CancellationToken cancellationToken)
		{
			var images = _client.Value.GetImageClient(_options.ImageModel);

			var options = new ImageGenerationOptions();
			// Only send what is configured: compatible servers differ in which (values of) parameters they accept.
			if (!string.IsNullOrEmpty(_options.Quality))
				options.Quality = new GeneratedImageQuality(_options.Quality);
			if (_options.Size is { } size)
				options.Size = new GeneratedImageSize(size.Width, size.Height);
			if (_options.RequestBase64)
				options.ResponseFormat = GeneratedImageFormat.Bytes;

			GeneratedImage image = await images.GenerateImageAsync(prompt, options, cancellationToken);

			if (image.ImageBytes is { } bytes)
				return bytes.ToArray();

			// Some servers (Together AI, LocalAI) return a (temporary) url by default
			return image.ImageUri is null
				? null
				: await _downloadClient.GetByteArrayAsync(image.ImageUri, cancellationToken);
		}
	}

	/// <summary>
	/// Configuration of an <see cref="OpenAICompatibleImageService"/>.
	/// </summary>
	public sealed record OpenAICompatibleOptions
	{
		/// <summary>The API base address including version, e.g. <c>https://api.together.xyz/v1</c>. <c>null</c> for OpenAI.</summary>
		public Uri Endpoint { get; init; }

		/// <summary>The API key, optional for local servers.</summary>
		public string ApiKey { get; init; }

		/// <summary>The image model, e.g. <c>gpt-image-2</c> or <c>black-forest-labs/FLUX.1-schnell</c>.</summary>
		public string ImageModel { get; init; }

		/// <summary>The chat model used to compose the image prompt, e.g. <c>gpt-5-mini</c>.</summary>
		public string ChatModel { get; init; }

		/// <summary>Optional image quality, e.g. <c>low</c>, <c>medium</c>, <c>high</c>. Not sent when empty.</summary>
		public string Quality { get; init; }

		/// <summary>Optional image size. Not sent when <c>null</c>.</summary>
		public (int Width, int Height)? Size { get; init; }

		/// <summary>Request base64 image data (<c>response_format=b64_json</c>) instead of a url. Not supported by OpenAI's gpt-image models.</summary>
		public bool RequestBase64 { get; init; }

		/// <summary>Name used in logging and profiling, defaults to the endpoint host.</summary>
		public string ProviderName { get; init; }

		/// <summary>
		/// Parses a size like <c>1024x768</c>, returns <c>null</c> for empty or invalid input.
		/// </summary>
		public static (int Width, int Height)? ParseSize(string size)
		{
			var parts = (size ?? string.Empty).Split('x', 'X', '×');
			return parts.Length == 2
				&& int.TryParse(parts[0], System.Globalization.CultureInfo.InvariantCulture, out var width)
				&& int.TryParse(parts[1], System.Globalization.CultureInfo.InvariantCulture, out var height)
				&& width > 0 && height > 0
				? (width, height)
				: null;
		}
	}
}
