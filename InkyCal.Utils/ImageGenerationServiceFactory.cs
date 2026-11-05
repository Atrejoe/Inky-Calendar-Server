using System;

namespace InkyCal.Utils
{
	/// <summary>
	/// Creates the <see cref="IImageGenerationService"/> selected by configuration setting
	/// <c>ImageGenerationProvider</c> (see <see cref="Server.Config.Config.ImageGenerationProvider"/>).
	/// </summary>
	/// <remarks>
	/// Supported providers:
	/// <list type="bullet">
	/// <item><c>OpenAI</c> (default): requires <c>OpenAIAPIKey</c>, optionally <c>OpenAIImageModel</c>.</item>
	/// <item><c>Mock</c>: returns placeholder images, no network calls.</item>
	/// </list>
	/// When the selected provider is not configured (e.g. missing API key) <see cref="IImageGenerationService.IsAvailable"/> is <c>false</c>,
	/// callers decide on a fallback (the server uses <see cref="MockImageGenerationService"/>, the calendar panel falls back to list mode).
	/// </remarks>
	public static class ImageGenerationServiceFactory
	{
		private static readonly Lazy<IImageGenerationService> _default = new(() => Create(Server.Config.Config.ImageGenerationProvider));

		/// <summary>
		/// The configured image generation service, shared so all callers share its rate limiting.
		/// </summary>
		public static IImageGenerationService Default => _default.Value;

		/// <summary>
		/// Creates the image generation service for <paramref name="provider"/>.
		/// </summary>
		/// <param name="provider">The provider name, case insensitive. Defaults to <c>OpenAI</c> when empty.</param>
		public static IImageGenerationService Create(string provider)
		{
			return (string.IsNullOrWhiteSpace(provider) ? "OpenAI" : provider.Trim()).ToUpperInvariant() switch
			{
				"OPENAI" => new OpenAIService(Server.Config.Config.OpenAIAPIKey, Server.Config.Config.OpenAIImageModel),
				"MOCK" => new MockImageGenerationService(),
				_ => throw new NotSupportedException($"Unknown image generation provider '{provider}'."),
			};
		}
	}
}
