using System;
using Microsoft.Extensions.Configuration;

namespace InkyCal.Server.Config
{
	public static class Config
	{
		private static readonly Lazy<IConfigurationRoot> configuration = new Lazy<IConfigurationRoot>(GetConfiguration);

		private static IConfigurationRoot GetConfiguration()
		{
			return new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables()
				.Build();
		}

		public static string ConnectionString => configuration.Value.GetConnectionString("DefaultConnection");
		public static string SentryDSN => configuration.Value.GetValue("SentryDSN", string.Empty);
		public static string BugSnagAPIKey => configuration.Value.GetValue("BugSnagAPIKey", string.Empty);
	}
}
