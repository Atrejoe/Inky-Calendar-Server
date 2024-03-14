using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace InkyCal.Server
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public static class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					if (!string.IsNullOrWhiteSpace(Config.Config.SentryDSN))
					{
						webBuilder.UseSentry(o =>
						{
							o.Dsn = Config.Config.SentryDSN;
							// Set TracesSampleRate to 1.0 to capture 100% of transactions for performance monitoring.
							// We recommend adjusting this value in production.
							o.TracesSampleRate = 0.01;
						});
					}

					webBuilder.UseStartup<Startup>();
				});
	}
}
