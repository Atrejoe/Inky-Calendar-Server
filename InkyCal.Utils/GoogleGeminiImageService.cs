using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

namespace InkyCal.Utils
{
	/// <summary>
	/// Generates images using Google's Gemini API, through Google's own .NET SDK (<c>Google.GenAI</c>).
	/// </summary>
	/// <remarks>
	/// <para>Uses Gemini image models (<c>gemini-*-image</c>, "Nano Banana") through <c>generateContent</c>.
	/// Imagen models are not supported: they were shut down in 2026 and the SDK only offers them on Vertex AI.</para>
	/// <para>The prompt is composed by a Gemini text model, so a single API key serves both steps.</para>
	/// </remarks>
	public sealed class GoogleGeminiImageService : ImageGenerationServiceBase
	{
		/// <summary>Image model used when none is configured: about $0.034 per 1K image (2026-11).</summary>
		public const string DefaultImageModel = "gemini-3.1-flash-lite-image";

		/// <summary>Text model used to compose the image prompt when none is configured.</summary>
		public const string DefaultChatModel = "gemini-3.1-flash-lite";

		/// <summary>Aspect ratio requested when none is configured. Most e-ink panels are landscape, around 5:3.</summary>
		public const string DefaultAspectRatio = "4:3";

		private readonly string _apiKey;
		private readonly string _imageModel;
		private readonly string _chatModel;
		private readonly string _aspectRatio;
		private readonly Lazy<Client> _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoogleGeminiImageService"/> class.
		/// When <paramref name="apiKey"/> is null or empty <see cref="IsAvailable"/> returns <c>false</c>.
		/// </summary>
		/// <param name="apiKey">The Gemini API key (Google AI Studio).</param>
		/// <param name="imageModel">The image model, defaults to <see cref="DefaultImageModel"/>.</param>
		/// <param name="chatModel">The text model used to compose the image prompt, defaults to <see cref="DefaultChatModel"/>.</param>
		/// <param name="aspectRatio">The requested aspect ratio, e.g. "1:1", "4:3", "16:9"; defaults to <see cref="DefaultAspectRatio"/>.</param>
		public GoogleGeminiImageService(string apiKey, string imageModel = null, string chatModel = null, string aspectRatio = null)
			: this(apiKey, imageModel, chatModel, aspectRatio, baseUrl: null)
		{
		}

		/// <summary>Initializes a new instance against a custom endpoint, used for testing.</summary>
		/// <param name="apiKey">The Gemini API key.</param>
		/// <param name="imageModel">The image model.</param>
		/// <param name="chatModel">The text model.</param>
		/// <param name="aspectRatio">The requested aspect ratio.</param>
		/// <param name="baseUrl">Overrides the Gemini API endpoint.</param>
		internal GoogleGeminiImageService(string apiKey, string imageModel, string chatModel, string aspectRatio, Uri baseUrl)
		{
			_apiKey = apiKey;
			_imageModel = string.IsNullOrWhiteSpace(imageModel) ? DefaultImageModel : imageModel;
			_chatModel = string.IsNullOrWhiteSpace(chatModel) ? DefaultChatModel : chatModel;
			_aspectRatio = string.IsNullOrWhiteSpace(aspectRatio) ? DefaultAspectRatio : aspectRatio;
			_client = new Lazy<Client>(() => new Client(
				apiKey: _apiKey,
				httpOptions: baseUrl is null ? null : new HttpOptions { BaseUrl = baseUrl.ToString() }));
		}

		/// <inheritdoc/>
		public override string ProviderName => "Google Gemini";

		/// <inheritdoc/>
		public override bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

		/// <inheritdoc/>
		protected override async Task<string> GetChatCompletionCoreAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
		{
			var response = await _client.Value.Models.GenerateContentAsync(
				model: _chatModel,
				contents: userPrompt,
				config: new GenerateContentConfig
				{
					SystemInstruction = new Content { Parts = [new Part { Text = systemPrompt }] },
				},
				cancellationToken: cancellationToken);

			return response.Text;
		}

		/// <inheritdoc/>
		protected override async Task<byte[]> GenerateImageCoreAsync(string prompt, CancellationToken cancellationToken)
		{
			var response = await _client.Value.Models.GenerateContentAsync(
				model: _imageModel,
				contents: prompt,
				config: new GenerateContentConfig
				{
					ResponseModalities = [Modality.Image.ToString()],
					ImageConfig = new ImageConfig { AspectRatio = _aspectRatio },
				},
				cancellationToken: cancellationToken);

			return response.Candidates?
				.SelectMany(c => c.Content?.Parts ?? [])
				.Select(p => p.InlineData?.Data)
				.FirstOrDefault(d => d is { Length: > 0 });
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (disposing && _client.IsValueCreated)
				_client.Value.Dispose();
			base.Dispose(disposing);
		}
	}
}
