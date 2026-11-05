using System;
using Microsoft.Extensions.Configuration;

namespace InkyCal.Server.Config
{
	public static class Config
	{
		private static readonly Lazy<IConfigurationRoot> configuration = new Lazy<IConfigurationRoot>(GetConfiguration);

		private static IConfigurationRoot GetConfiguration()
		{
			var result = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.AddUserSecrets(typeof(Config).Assembly, optional: true, reloadOnChange: true)
				.Build();

			result.GetSection("GoogleOAuth").Bind(new GoogleOAuth());

			return result;
		}

		public static string ConnectionString => configuration.Value.GetConnectionString("DefaultConnection");
		public static string SentryDSN => configuration.Value.GetValue(nameof(SentryDSN), string.Empty);
		public static double SentryTracesSampleRate => configuration.Value.GetValue(nameof(SentryTracesSampleRate), 0.01);
		public static double SentryPanelTracesSampleRate => configuration.Value.GetValue(nameof(SentryPanelTracesSampleRate), 1.0);
		public static string BugSnagAPIKey => configuration.Value.GetValue(nameof(BugSnagAPIKey), string.Empty);
		public static bool TraceQueries => configuration.Value.GetValue(nameof(TraceQueries), false);

		/// <summary>
		/// The image generation provider used for AI-image calendar panels, e.g. <c>OpenAI</c> (default), <c>Gemini</c> or <c>Mock</c>.
		/// </summary>
		public static string ImageGenerationProvider => configuration.Value.GetValue(nameof(ImageGenerationProvider), "OpenAI");

		public static string OpenAIAPIKey => configuration.Value.GetValue(nameof(OpenAIAPIKey), string.Empty);
		/// <summary>The OpenAI image model, e.g. <c>gpt-image-2</c>. Empty uses the default of the OpenAI image generation service.</summary>
		public static string OpenAIImageModel => configuration.Value.GetValue(nameof(OpenAIImageModel), string.Empty);
		/// <summary>Google Gemini API key (Google AI Studio), used by image generation provider <c>Gemini</c>.</summary>
		public static string GeminiAPIKey => configuration.Value.GetValue(nameof(GeminiAPIKey), string.Empty);
		/// <summary>Gemini image model, e.g. <c>gemini-3.1-flash-lite-image</c>. Empty uses the default.</summary>
		public static string GeminiImageModel => configuration.Value.GetValue(nameof(GeminiImageModel), string.Empty);
		/// <summary>Gemini text model used to compose the image prompt. Empty uses the default.</summary>
		public static string GeminiChatModel => configuration.Value.GetValue(nameof(GeminiChatModel), string.Empty);
		/// <summary>Requested aspect ratio of Gemini images, e.g. <c>4:3</c>. Empty uses the default.</summary>
		public static string GeminiAspectRatio => configuration.Value.GetValue(nameof(GeminiAspectRatio), string.Empty);

		public static string OpenWeatherAPIKey => configuration.Value.GetValue(nameof(OpenWeatherAPIKey), string.Empty);

	}
}
