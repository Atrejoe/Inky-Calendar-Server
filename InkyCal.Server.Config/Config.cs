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
		/// The image generation provider used for AI-image calendar panels, e.g. <c>OpenAI</c> (default), <c>OpenAICompatible</c> or <c>Mock</c>.
		/// </summary>
		public static string ImageGenerationProvider => configuration.Value.GetValue(nameof(ImageGenerationProvider), "OpenAI");

		public static string OpenAIAPIKey => configuration.Value.GetValue(nameof(OpenAIAPIKey), string.Empty);
		/// <summary>The OpenAI image model, e.g. <c>gpt-image-2</c>. Empty uses the default of the OpenAI image generation service.</summary>
		public static string OpenAIImageModel => configuration.Value.GetValue(nameof(OpenAIImageModel), string.Empty);
		/// <summary>The OpenAI chat model used to compose the image prompt. Empty uses the default.</summary>
		public static string OpenAIChatModel => configuration.Value.GetValue(nameof(OpenAIChatModel), string.Empty);

		/// <summary>Base address of an OpenAI-compatible API including version, e.g. <c>https://api.together.xyz/v1</c> or <c>http://localai:8080/v1</c>.</summary>
		public static string OpenAICompatibleEndpoint => configuration.Value.GetValue(nameof(OpenAICompatibleEndpoint), string.Empty);
		/// <summary>API key of the OpenAI-compatible API, optional for local servers.</summary>
		public static string OpenAICompatibleAPIKey => configuration.Value.GetValue(nameof(OpenAICompatibleAPIKey), string.Empty);
		/// <summary>Image model of the OpenAI-compatible API, e.g. <c>black-forest-labs/FLUX.1-schnell</c>.</summary>
		public static string OpenAICompatibleImageModel => configuration.Value.GetValue(nameof(OpenAICompatibleImageModel), string.Empty);
		/// <summary>Chat model of the OpenAI-compatible API used to compose the image prompt.</summary>
		public static string OpenAICompatibleChatModel => configuration.Value.GetValue(nameof(OpenAICompatibleChatModel), string.Empty);
		/// <summary>Optional image quality (<c>low</c>, <c>medium</c>, <c>high</c>), not sent when empty.</summary>
		public static string OpenAICompatibleImageQuality => configuration.Value.GetValue(nameof(OpenAICompatibleImageQuality), string.Empty);
		/// <summary>Optional image size, e.g. <c>1024x768</c>, not sent when empty.</summary>
		public static string OpenAICompatibleImageSize => configuration.Value.GetValue(nameof(OpenAICompatibleImageSize), string.Empty);
		/// <summary>Request base64 image data instead of a url.</summary>
		public static bool OpenAICompatibleRequestBase64 => configuration.Value.GetValue(nameof(OpenAICompatibleRequestBase64), false);

		public static string OpenWeatherAPIKey => configuration.Value.GetValue(nameof(OpenWeatherAPIKey), string.Empty);

	}
}
