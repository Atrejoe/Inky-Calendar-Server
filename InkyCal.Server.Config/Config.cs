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
		public static string BugSnagAPIKey => configuration.Value.GetValue(nameof(BugSnagAPIKey), string.Empty);
		public static bool TraceQueries => configuration.Value.GetValue(nameof(TraceQueries), false);

		public static string OpenAIAPIKey => configuration.Value.GetValue(nameof(OpenAIAPIKey), string.Empty);
		public static string OpenWeatherAPIKey => configuration.Value.GetValue(nameof(OpenWeatherAPIKey), string.Empty);

		/// <summary>
		/// Gets the cache type (Memory or Redis). Default is Memory.
		/// </summary>
		public static CacheType CacheType => configuration.Value.GetValue("Cache:Type", CacheType.Memory);

		/// <summary>
		/// Gets the Redis connection string for caching.
		/// </summary>
		public static string RedisCacheConnectionString => configuration.Value.GetValue("Cache:Redis:ConnectionString", string.Empty);

		/// <summary>
		/// Gets the memory cache size limit in bytes. Default is 500 MB.
		/// </summary>
		public static long MemoryCacheSizeLimit => configuration.Value.GetValue("Cache:Memory:SizeLimit", 1024L * 1024 * 500);
	}
}
