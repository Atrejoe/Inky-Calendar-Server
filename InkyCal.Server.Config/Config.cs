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

		public static string OpenAIAPIKey => configuration.Value.GetValue(nameof(OpenAIAPIKey), string.Empty);
		public static int OpenAIImagePermitLimit => configuration.Value.GetValue(nameof(OpenAIImagePermitLimit), 2);
		public static int OpenAIImageRateLimitWindowSeconds => configuration.Value.GetValue(nameof(OpenAIImageRateLimitWindowSeconds), 30);
		public static string OpenWeatherAPIKey => configuration.Value.GetValue(nameof(OpenWeatherAPIKey), string.Empty);

	}
}
