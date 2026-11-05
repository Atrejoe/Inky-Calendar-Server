namespace InkyCal.Utils
{
	/// <summary>
	/// Generates images using the OpenAI API, through OpenAI's own .NET SDK.
	/// </summary>
	/// <remarks>
	/// The image model is configurable (setting <c>OpenAIImageModel</c>), gpt-image-1 was shut down by OpenAI on 2026-10-23.
	/// </remarks>
	public sealed class OpenAIService : OpenAICompatibleImageService
	{
		/// <summary>The image model used when none is configured.</summary>
		public const string DefaultImageModel = "gpt-image-2";

		/// <summary>The chat model used to compose the image prompt when none is configured.</summary>
		public const string DefaultChatModel = "gpt-5-mini";

		/// <summary>
		/// Initialises the service with the given API key.
		/// When <paramref name="apiKey"/> is null or empty <see cref="OpenAICompatibleImageService.IsAvailable"/> returns <c>false</c>.
		/// </summary>
		/// <param name="apiKey">The OpenAI API key.</param>
		/// <param name="imageModel">The image model, defaults to <see cref="DefaultImageModel"/>.</param>
		/// <param name="chatModel">The chat model, defaults to <see cref="DefaultChatModel"/>.</param>
		public OpenAIService(string apiKey, string imageModel = null, string chatModel = null)
			: base(new OpenAICompatibleOptions
			{
				ApiKey = apiKey,
				ImageModel = string.IsNullOrWhiteSpace(imageModel) ? DefaultImageModel : imageModel,
				ChatModel = string.IsNullOrWhiteSpace(chatModel) ? DefaultChatModel : chatModel,
				// Medium quality: low quality yields noticeably rougher images, high quality is mostly lost when dithering down
				// to the few colors of an e-ink display. Images are cached for hours, so generation time and cost matter little.
				Quality = "medium",
				// gpt-image-* models always return base64 and reject 'response_format'
				RequestBase64 = false,
				ProviderName = "OpenAI",
			})
		{
		}
	}
}
