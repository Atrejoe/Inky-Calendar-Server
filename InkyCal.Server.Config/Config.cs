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
		/// The image generation provider used for AI-image calendar panels, e.g. <c>OpenAI</c> (default), <c>BlackForestLabs</c> or <c>Mock</c>.
		/// </summary>
		public static string ImageGenerationProvider => configuration.Value.GetValue(nameof(ImageGenerationProvider), "OpenAI");

		public static string OpenAIAPIKey => configuration.Value.GetValue(nameof(OpenAIAPIKey), string.Empty);
		/// <summary>The OpenAI image model, e.g. <c>gpt-image-2</c>. Empty uses the default of the OpenAI image generation service.</summary>
		public static string OpenAIImageModel => configuration.Value.GetValue(nameof(OpenAIImageModel), string.Empty);
		/// <summary>Black Forest Labs API key, used by image generation provider <c>BlackForestLabs</c>.</summary>
		public static string BlackForestLabsAPIKey => configuration.Value.GetValue(nameof(BlackForestLabsAPIKey), string.Empty);
		/// <summary>FLUX.2 model, e.g. <c>flux-2-klein-9b</c> or <c>flux-2-pro</c>. Empty uses the default.</summary>
		public static string BlackForestLabsModel => configuration.Value.GetValue(nameof(BlackForestLabsModel), string.Empty);
		/// <summary>FLUX.2 image size, e.g. <c>1024x768</c>. Empty uses the default.</summary>
		public static string BlackForestLabsImageSize => configuration.Value.GetValue(nameof(BlackForestLabsImageSize), string.Empty);
		/// <summary>The image generation provider whose language model composes the FLUX prompt (default <c>OpenAI</c>), <c>None</c> to pass the prompt as-is.</summary>
		public static string BlackForestLabsPromptProvider => configuration.Value.GetValue(nameof(BlackForestLabsPromptProvider), "OpenAI");

		public static string OpenWeatherAPIKey => configuration.Value.GetValue(nameof(OpenWeatherAPIKey), string.Empty);

	}
}
